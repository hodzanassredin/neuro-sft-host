import os
import argparse
import requests
import wget
from bs4 import BeautifulSoup
from urllib.parse import urljoin

def extract_links(url):
    """
    Extract all links from the given URL.

    :param url: The URL to extract links from.
    :return: A list of extracted links.
    """
    try:
        response = requests.get(url, verify=False)
        response.raise_for_status()
    except requests.RequestException as e:
        print(f"Failed to access the page. Error: {e}")
        return []

    soup = BeautifulSoup(response.content, 'html.parser')
    links = [link.get('href') for link in soup.find_all('a') if link.get('href')]
    return links

def filter_pdf_links(links, base_url):
    """
    Filter the links to include only those that end with '.pdf'.

    :param links: A list of links to filter.
    :param base_url: The base URL to join with relative links.
    :return: A list of PDF links.
    """
    pdf_links = [urljoin(base_url, link) for link in links if link.endswith('.pdf')]
    return pdf_links

def download_links(pdf_links, folder):
    """
    Download and save all PDF files from the given links.

    :param pdf_links: A list of PDF links to download.
    :param folder: The folder to save the downloaded PDF files.
    """
    if not os.path.exists(folder):
        os.makedirs(folder)

    for link in pdf_links:
        try:
            wget.download(link, out=folder)
            print(f'Downloaded {link}')
        except Exception as e:
            print(f'Failed to download {link}. Error: {e}')

def main(url, folder):
    """
    Main function to extract, filter, and download PDF links.

    :param url: The URL to start extracting links from.
    :param folder: The folder to save the downloaded PDF files.
    """
    base_url = url
    pdf_links = filter_pdf_links(extract_links(url), base_url)
    download_links(pdf_links, folder)

    # Process additional pages
    page_links = extract_links(url)
    pages = [urljoin(base_url, l) for l in page_links if l.startswith('/library/')]

    for page_link in pages:
        print(f'Processing {page_link}')
        pdf_links = filter_pdf_links(extract_links(page_link), base_url)
        download_links(pdf_links, folder)

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Download PDF files from a given URL.')
    parser.add_argument('url', type=str, help='The URL to start extracting links from.')
    parser.add_argument('--folder', type=str, default='pdf_files', help='The folder to save the downloaded PDF files.')

    args = parser.parse_args()
    main(args.url, args.folder)
