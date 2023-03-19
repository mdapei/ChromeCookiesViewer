# ChromeCookiesViewer
ChromeCookiesViewer is a Windows command-line tool that allows you to export or delete the cookies stored by Google Chrome Web Browser.

## Downloads

Download from [releases](https://github.com/mdapei/ChromeCookiesViewer/releases)

## Development

- IDE: [Visual Studio 2019](https://learn.microsoft.com/it-it/visualstudio/releases/2019/release-notes)
- Language: C# 7.3
- SDK: .NET Framework 4.7.2

### Build

1. Clone the repository.
```
$ git clone https://github.com/mdapei/ChromeCookiesViewer.git
```
2. Open the repository in VS2019, switch to the _Release_ configuration, and build the solution. That's it!

### Usage
```
ChromeCookiesViewer <host> [-s <file> | -p | -d] [OPTIONS]

   <host>        Regex for filtering the host name associated to the cookies.
                 Use the asterisk (*) wildcard for all hosts.

   -s <file>     Save the cookies in the specified text file (path can be absolute or relative).

   -p            Print cookies in console.

   -d            Delete cookies from the browser (Google Chrome must be closed).

Options:

   -n <name>     Regex for filtering the cookie names.

   -a            Include multiple cookies with the same name.
                 Without this option, in case of cookies with the same name,
                 only the most up-to-date one will be extracted.

   -e            Exclude expired cookies.
```

## Support
GitHub Issues are for Bugs and Feature Requests Only

## License
- [MIT License](https://spdx.org/licenses/MIT.html)
