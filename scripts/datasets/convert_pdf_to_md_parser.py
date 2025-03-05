import argparse
import re
from lark import Lark, UnexpectedEOF

# Constants
MAX_TEXT_SIZE = 512
MD_CODE_START = "```oberon\n"
MD_CODE_END = "```\n"

# EBNF Grammar for parsing
ebnf_grammar = """
    document_t: module [TAIL]
    procedure_t: proc_decl [TAIL]
    statement_seq_t: statement_seq [TAIL]
    decl_seq_t: decl_seq [TAIL]

    module: MODULE IDENT ";" (import_list)? decl_seq (BEGIN statement_seq)? (CLOSE statement_seq)? END IDENT "."
    import_list: IMPORT (IDENT ":=")? IDENT ("," (IDENT ":=")? IDENT)* ";"
    decl_seq: (CONST (const_decl ";")* | TYPE (type_decl ";")* | VAR (var_decl ";")*)* (proc_decl ";" | forward_decl ";")*
    const_decl: ident_def "=" const_expr
    type_decl: ident_def "=" type
    var_decl: ident_list ":" type
    proc_decl: PROCEDURE (receiver)? ident_def (formal_pars)? meth_attributes (";" decl_seq (BEGIN statement_seq)? END IDENT)?
    meth_attributes: ("," NEW)? ("," (ABSTRACT | EMPTY | EXTENSIBLE))?
    forward_decl: PROCEDURE "^" (receiver)? ident_def (formal_pars)? meth_attributes
    formal_pars: "(" (f_p_section (";" f_p_section)*)? ")" (":" type)?
    f_p_section: (VAR | IN | OUT)? IDENT ("," IDENT)* ":" type
    receiver: "(" (VAR | IN)? IDENT ":" IDENT ")"
    type: qualident | ARRAY (const_expr ("," const_expr)*)? OF type | (ABSTRACT | EXTENSIBLE | LIMITED)? | RECORD ("(" qualident ")")? field_list (";" field_list)* END | POINTER TO type | PROCEDURE (formal_pars)?
    field_list: (ident_list ":" type)?
    statement_seq: (statement)? (";" (statement)?)*
    statement: designator ":=" expr
            | IF expr THEN statement_seq (ELSIF expr THEN statement_seq)* (ELSE statement_seq)? END
            | CASE expr OF case ("|" case)* (ELSE statement_seq)? END
            | WHILE expr DO statement_seq END
            | REPEAT statement_seq UNTIL expr
            | FOR IDENT ":=" expr TO expr ("BY" const_expr)? DO statement_seq END
            | LOOP statement_seq END
            | WITH (guard DO statement_seq)? ("|" (guard DO statement_seq)?)* (ELSE statement_seq)? END
            | EXIT
            | RETURN (expr)?
            | designator ("(" (expr_list)? ")")?

    case: (case_labels ("," case_labels)* ":" statement_seq)?
    case_labels: const_expr (".." const_expr)?
    guard: qualident ":" qualident
    const_expr: expr
    expr: simple_expr (relation simple_expr)?
    simple_expr: ("+" | "-")? term (add_op term)*
    term: factor (mul_op factor)*
    factor: designator | NUMBER | CHARACTER | STRING | NIL | set | "(" expr ")" | " ~ " factor
    set: "{" (element ("," element)*)? "}"
    element: expr (".." expr)?
    relation: "=" | "#" | "<" | "<=" | ">" | ">=" | IN | IS
    add_op: "+" | "-" | OR
    mul_op: "*" | "/" | DIV | MOD | "&"
    designator: qualident ("." IDENT | "[" expr_list "]" | " ^ " | "(" qualident ")" | "(" (expr_list)? ")")* ("$")?
    expr_list: expr ("," expr)*
    ident_list: ident_def ("," ident_def)*
    qualident: (IDENT ".")? IDENT
    ident_def: IDENT ("*" | "-")?
    TAIL: /[\s\S]+/
    ANY: /[^ ]+/

    NUMBER: INT | FLOAT | HEX
    HEX: /0x[0-9A-Fa-f]+/
    CHARACTER: /'[^']*'/
    STRING: /"[^"]*"/

    IN.1: "IN"
    IS.1: "IS"
    OR.1: "OR"
    DIV.1: "DIV"
    MOD.1: "MOD"
    NIL.1: "NIL"
    VAR.1: "VAR"
    CONST.1: "CONST"
    TYPE.1: "TYPE"
    PROCEDURE.1: "PROCEDURE"
    ABSTRACT.1: "ABSTRACT"
    EMPTY.1: "EMPTY"
    EXTENSIBLE.1: "EXTENSIBLE"
    LIMITED.1: "LIMITED"
    RECORD.1: "RECORD"
    POINTER.1: "POINTER"
    TO.1: "TO"
    IF.1: "IF"
    THEN.1: "THEN"
    ELSIF.1: "ELSIF"
    ELSE.1: "ELSE"
    CASE.1: "CASE"
    OF.1: "OF"
    WHILE.1: "WHILE"
    DO.1: "DO"
    REPEAT.1: "REPEAT"
    UNTIL.1: "UNTIL"
    FOR.1: "FOR"
    LOOP.1: "LOOP"
    WITH.1: "WITH"
    EXIT.1: "EXIT"
    RETURN.1: "RETURN"
    BEGIN.1: "BEGIN"
    CLOSE.1: "CLOSE"
    END.1: "END"
    IMPORT.1: "IMPORT"
    NEW.1: "NEW"
    MODULE.1: "MODULE"
    OUT.1: "OUT"
    ARRAY.1: "ARRAY"
    IDENT.0: CNAME
    COMMENT: /\(\*([^*]|\*+[^*)])*\*+\)/
    %import common.NEWLINE
    %import common.CNAME
    %import common.INT
    %import common.FLOAT
    %import common.WS

    %ignore WS
    %ignore COMMENT
"""

comment_grammar = """
    _comment: COMMENT?
    COMMENT: /\(\*([^*]|\*+[^*)])*\*+\)/
    %import common.WS
    %import common.NEWLINE
    %ignore WS
"""

# Define the starts for the parsers
starts = [
    "document_t",
    "procedure_t",
    "statement_seq_t",
    "decl_seq_t"
]

# Initialize parsers for each start rule
parsers = [Lark(ebnf_grammar, start=s) for s in starts]


def is_really_code(text):
    """
    Check if the text is likely to be code.
    """
    return len(text) > 10 and len(text.split()) > 1


def get_code_and_tail(orig_text, max_size=MAX_TEXT_SIZE):
    """
    Extract code and tail from the original text.
    """
    for parser in parsers:
        try:
            text = orig_text[:max_size] \
                    if len(orig_text) > max_size else orig_text
            tree = parser.parse(text)
            split_idx = len(text) - len(tree.children[1].value)

            code = orig_text[:split_idx]
            if is_really_code(code):
                return code, orig_text[split_idx:]
        except UnexpectedEOF:
            if len(orig_text) > max_size:
                return get_code_and_tail(orig_text, max_size * 2)
            else:
                return orig_text, ""  # whole text is code
        except Exception:
            pass
    return "", orig_text  # no code


def load_file(path):
    """
    Load the content of a file.
    """
    with open(path, 'r', encoding='utf-8') as f:
        return f.read()


def escape_markdown(text):
    """
    Escape special Markdown characters.
    """
    escape_chars = r'\_*[]()~`>#+-=|{}.!'
    return re.sub(f'([{re.escape(escape_chars)}])', r'\\\1', text)


def find_first_line_end(text):
    """
    Find the end of the first line in the text.
    """
    if not text or text.isspace():
        return len(text)
    nl_idx = 0
    while nl_idx < len(text) and text[nl_idx].isspace():
        nl_idx += 1
    nl_idx = text.find('\n', nl_idx)
    if nl_idx == -1:
        return len(text)
    return nl_idx + 1 if nl_idx != len(text) else nl_idx


def code_to_md(text):
    """
    Convert code to Markdown format.
    """
    is_code_started = False
    md = ""
    while text and not text.isspace():
        code, text = get_code_and_tail(text)
        if code:
            if not is_code_started:
                md += '\n' + MD_CODE_START + '\n'
                is_code_started = True
            md += code
        else:
            if is_code_started:
                md += '\n' + MD_CODE_END + '\n'
                is_code_started = False
            nl_idx = find_first_line_end(text)
            md += text[:nl_idx]
            text = text[nl_idx:]
    return md


def main(input_file, output_file):
    """
    Main function to process the file and write the result to the output file.
    """
    text = load_file(input_file)
    md = code_to_md(text)
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(md)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description="Convert text to Markdown format.")
    parser.add_argument("input_file", 
                        help="Path to the input file.")
    parser.add_argument("output_file", 
                        help="Path to the output Markdown file.")
    args = parser.parse_args()

    main(args.input_file, args.output_file)
