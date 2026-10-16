
# AI Usage Notes

## Tools used

**Claude:** explained compiler and runtime errors, discussed the project structure, and suggested example test cases.
**Codex:** drafted terminal commands for building and pushing the frontend and the test project.
- AI suggestions also helped make the React frontend more user-friendly, improve its styling, and add explanatory comments.
- AI helped draft this README and these usage notes.
- I used the terminal and IDE to inspect the project, run commands, and verify results. Every AI suggestion was checked against the source code and the compiler/test output before I kept it.

## A place AI was wrong or unhelpful

An AI suggestion used `TransferItemDto` in the tests, but that type did not exist in this codebase. The actual DTO is `CreateTransferLineDto`, declared in `StockTransferApi/DTOs/CreateTransferDto.cs`. I noticed the mismatch from the compiler's `CS0246` error, searched the API and test files for both names, and corrected the test references to match the DTO used by the controller.


## Time spent

Approximately **3 hours** across implementation, debugging, test work, and documentation. 
