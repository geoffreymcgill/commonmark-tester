# CommonMark Tester

This project is a small .NET console app used to compare Markdig's HTML output against the official CommonMark spec test suite.

It downloads the CommonMark JSON test cases from the spec site, renders each Markdown example with Markdig, and compares the generated HTML to the expected CommonMark output.

The current focus of this repro is a CommonMark compliance difference in list rendering. Markdig is very close to full compliance in this test run, but it fails when a list item contains block-level content and the renderer omits the required newline immediately after `<li>`.

What this project is testing:

- CommonMark spec example conformance using the official spec JSON
- Markdig HTML output versus expected CommonMark HTML
- A specific list-rendering scenario where block content inside `<li>` is emitted on the same line as the opening list item tag

Related upstream issue:

- Markdig issue #933: https://github.com/xoofx/markdig/issues/933

