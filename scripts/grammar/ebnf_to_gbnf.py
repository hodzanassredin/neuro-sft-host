from lark import Lark, Tree, Token
import argparse
from typing import List

# EBNF grammar for Lark
ebnf_grammar = """
    start: rule+
    rule: ID "=" expression "."
    expression: term ("|" term)*
    term: factor+
    factor: ID
          | STRING
          | grouping
          | optional
          | repetition
          | range
    grouping: "(" expression ")"
    optional: "[" expression "]"
    repetition: "{" expression "}"
    ID: /[a-zA-Z_][a-zA-Z0-9_]*/
    STRING: (/"[^"]*"/) | (/'[^']*'/)
    range: STRING "." "." STRING
    %import common.WS
    %ignore WS
"""


def rebuild_expression(expression_node: Tree, 
                       rule_names: List[str], 
                       wrap: bool = False) -> str:
    """Rebuilds an expression node into a string representation."""
    terms = [rebuild_term(term_node, rule_names) for term_node 
             in expression_node.children]
    joined = " | ".join(terms)
    return f"({joined})" if wrap else joined


def rebuild_term(term_node: Tree, rule_names: List[str]) -> str:
    """Rebuilds a term node into a string representation."""
    factors = [rebuild_factor(factor_node, rule_names) for factor_node 
               in term_node.children]
    return " ".join(factors)


def rebuild_optional(node: Tree, rule_names: List[str]) -> str:
    """Rebuilds an optional node into a string representation."""
    expr = rebuild_expression(node.children[0], rule_names, False)
    return f"({expr})?"


def rebuild_grouping(node: Tree, rule_names: List[str]) -> str:
    """Rebuilds a grouping node into a string representation."""
    expr = rebuild_expression(node.children[0], rule_names, False)
    return f"({expr})"


def rebuild_repetition(node: Tree, rule_names: List[str]) -> str:
    """Rebuilds a repetition node into a string representation."""
    expr = rebuild_expression(node.children[0], rule_names, False)
    return f"({expr})*"


def rebuild_range(node: Tree, rule_names: List[str]) -> str:
    """Rebuilds a range node into a string representation."""
    return f"[{node.children[0].value.strip('"')}-\
        {node.children[1].value.strip('"')}]"


def rebuild_factor(factor_node: Tree, rule_names: List[str]) -> str:
    """Rebuilds a factor node into a string representation."""
    result = []
    for child in factor_node.children:
        if isinstance(child, Tree):
            if child.data == "optional":
                result.append(rebuild_optional(child, rule_names))
            elif child.data == "grouping":
                result.append(rebuild_grouping(child, rule_names))
            elif child.data == "repetition":
                result.append(rebuild_repetition(child, rule_names))
            elif child.data == "range":
                result.append(rebuild_range(child, rule_names))
        elif isinstance(child, Token):
            if child.value in rule_names:
                result.append(child.value)
            elif child.value.startswith("\""):
                result.append(child.value)
            else:
                result.append(f"\"{child.value}\"")
    return "".join(result)


def rebuild_rule(rule_node: Tree, rule_names: List[str]) -> str:
    """Rebuilds a rule node into a string representation."""
    id_node = rule_node.children[0]
    expression_node = rule_node.children[1]
    return f"{id_node.value} ::= {rebuild_expression(expression_node, 
                                                     rule_names)}"


def rebuild_ebnf(tree: Tree) -> str:
    """Rebuilds the entire EBNF tree into a string representation."""
    rule_names = [c.children[0].value for c in tree.children 
                  if c.data == 'rule']
    result = [rebuild_rule(child, rule_names) if child.data == "rule" 
              else rebuild_expression(child, rule_names) for child 
              in tree.children]
    return "\n".join(result)


# Parser
ebnf_parser = Lark(ebnf_grammar, start='start')


def convert(file_name: str, out_file: str) -> None:
    """Converts an EBNF file to BNF and writes the output to a specified file."""
    try:
        with open(file_name, 'r', encoding='utf-8') as file:
            content = file.read()
            parsed = ebnf_parser.parse(content)
            result = rebuild_ebnf(parsed)

            # Write the result to the output file
            with open(out_file, 'w', encoding='utf-8') as output_file:
                output_file.write(result)
            print(f"Output written to {out_file}")
    except FileNotFoundError:
        print(f"File '{file_name}' not found.")
    except Exception as e:
        print(f"ERROR: {e}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Convert EBNF to BNF.")
    parser.add_argument("file_name", help="The EBNF file to convert.")
    parser.add_argument("out_file", help="The output file to write the BNF to.")
    args = parser.parse_args()

    convert(args.file_name, args.out_file)
