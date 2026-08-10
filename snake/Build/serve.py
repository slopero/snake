import functools
import os
import sys
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer

PORT = int(sys.argv[1]) if len(sys.argv) > 1 else 8000
ROOT = sys.argv[2] if len(sys.argv) > 2 else os.getcwd()


class Handler(SimpleHTTPRequestHandler):
    def __init__(self, *args, directory=None, **kwargs):
        super().__init__(*args, directory=directory, **kwargs)

    def end_headers(self):
        path = self.path.split("?")[0]
        if path.endswith(".gz"):
            self.send_header("Content-Encoding", "gzip")
        elif path.endswith(".br"):
            self.send_header("Content-Encoding", "br")
        super().end_headers()


handler = functools.partial(Handler, directory=ROOT)

with ThreadingHTTPServer(("", PORT), handler) as httpd:
    print(f"Serving {ROOT} on http://0.0.0.0:{PORT}")
    httpd.serve_forever()
