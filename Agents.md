## Core Rules

- When writing something intended for human consumption, (comment, commit message, reply to prompt) use as few words as possible. Pick every word meticulously to reduce the volume
  to a strict minimum. Be down to the point. Less is more.
- Code stays normal. English gets compressed.
- Output sounds human. Never AI-generated.
- No filler, no preamble, no pleasantries.
- Avoid superlatives and praise. Stop telling me I am absolutely right. Give me the cold hard truth.
- Never use em-dashes or replacement hyphens.
- Avoid parenthetical clauses entirely.
- Hyphens map to standard grammar only.
- Tool first. Result first.
- No explain unless asked.
- No action on question, respond first and wait for decision.
- Think before acting. Read existing files before writing code.
- Be concise in output but thorough in reasoning.
- Prefer editing over rewriting whole files.
- Do not re-read files you have already read unless the file may have changed.
- Skip files over 100KB unless explicitly required.
- Suggest running /cost when a session is running long to monitor cache ratio.
- Recommend starting a new session when switching to an unrelated task.
- Test your code before declaring done.
- No sycophantic openers or closing fluff.
- Keep solutions simple and direct.
- User instructions always override this file.

## Practices

- Follow the existing architecture in the project. Look for ADRs, if there aren't any, derive them based on the existing code.
- Avoid magic numbers and strings by extracting recurring or meaningful values into descriptive constants (const) or enums. Keep self-explanatory, one-off values inline to avoid
  clutter. If a value comes from a spec (e.g. HTTP 200 OK), use a constant regardless.
- Reduce code indentation. Avoid Arrow Anti-Pattern. Leverage early return and continue.
- Keep function names short. Less than 30 characters. (Does not apply to test method names.)
- Use enums instead of booleans for function parameters.
- No interface until at least two implementations exist.
- Let the reader of the code breathe. Add empty lines between logical blocks of code.
- Treat member visibility changes as a breaking design shift. Keep all fields and functions private unless external access is strictly required by the design. Prompt the user for
  explicit approval before changing any access modifier from private to internal or public.
- Program to levels of abstraction. Lower-level mechanics (e.g., raw hardware I/O, sector parsing, direct socket streams) must be encapsulated in a dedicated driver/abstraction
  layer. Expose clean, high-level APIs to the rest of the application so calling code works with domain concepts, not raw implementation details.
- Don't touch blocks of code unrelated to the feature you implement. e.g. Don't add comments to a block of code if you did not create it or modify it. As much as possible try to
  minimize the number of changed lines when implementing a feature.
- Strictly adhere to the layered boundary hierarchy: each layer may only communicate with its immediate neighbor directly below it. Never "punch holes" through layers (e.g.,
  controllers or UI components must never directly call database queries, raw hardware drivers, or low-level network clients; always route through the intermediate
  service/abstraction layer).
- Use TDD - usually user writes test, you write implementation.
- Never write test and implementation at the same time, test that fails first, then implementation that fixes it.
- If the prompt indicates that a bug is being fixed, don't write the fix right away. First write the test. Observe it failing. Then write the fix. And observe the test passing.
- When adding or removing an endpoint, update the .http test file.
- When you write a commit message, follow these 7 rules:
    - Rule 1: Always start with task number from branch name (#xxxxx), separate the subject line from the body with a single blank line.
    - Rule 2: Limit the subject line to 50 characters (72 is the absolute hard limit).
    - Rule 3: Capitalize the first letter of the subject line.
    - Rule 4: Do not end the subject line with a period.
    - Rule 5: Use the imperative mood in the subject line (e.g., "Fix bug," "Add feature," not "Fixed" or "Adds"). Test formula: It must complete the sentence: "If applied, this
      commit will [your subject line here]".
    - Rule 6: Wrap the body text manually at 72 characters to prevent Git formatting issues.
    - Rule 7: Use the body to explain what and why vs. how. Assume the code explains the how; the message must explain the context and reasoning.

## Code styles

- No empty lines at end of files.