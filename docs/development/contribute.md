# Contribute to Libation

We welcome contributions! Whether it's fixing bugs, adding features, or improving documentation, your help is appreciated.

> [!WARNING]
> Read the [Development - Getting Started](/docs/development/getting-started) guide first.

## Getting Started

1.  **Fork the repository** on GitHub.
2.  **Clone your fork** locally.
3.  **Create a branch** for your feature or fix:
    ```bash
    git checkout -b feature/my-new-feature
    ```

## Code Style

- Follow standard C# coding conventions.
- Ensure your code builds and runs without errors.
- Clean up any unused dependencies or imports.

## Logging and secrets

We ask people to attach `Log.log` to public issue reports, so treat everything written there as published.

- Log `account.MaskedLogEntry`, never an account's id or name. `AccountCredentialStatus.FormatAccountLabel` gives the unmasked label and is for dialogs shown to the account's owner only.
- Never hang an `Account`, an `Identity`, or anything holding one off an exception. Serilog.Exceptions reflects over every public property of a logged exception and follows nested objects, so a live account on an exception publishes its address and activation bytes no matter what `ToString` says. Carry an `AccountSummary` instead. A test enforces this: see `ExceptionsCannotReachAnAccount`.
- Remember that an exception's `Message` gets logged too, so mask anything you interpolate into one.
- Wrap a new secret in `Dinah.Core.Security.SecretString`, which keeps the value behind `Reveal()` where reflection cannot find it and prints `[REDACTED length=N]` everywhere else. Implement `ILogMasked` on a type that needs a masked identity in logs.
- `Reveal()` at the point of use, and nowhere else. Interpolating a secret into a string is not a compile error, so a redaction can end up sent over the wire in place of the real value - cover any new call site with a test.

## Submitting a Pull Request

1.  **Commit your changes** with a clear message.
2.  **Push to your fork**:
    ```bash
    git push origin feature/my-new-feature
    ```
3.  **Open a Pull Request** on the main repository.
4.  Describe your changes and link any related issues.

## Reporting Issues

If you find a bug or have a feature request, please [open an issue](https://github.com/rmcrackan/Libation/issues) on GitHub.
