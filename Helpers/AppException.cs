using System;
using System.Globalization;
using System.Net;

namespace atmnr_api.Helpers;
// custom exception class for throwing application specific exceptions (e.g. for validation) 
// that can be caught and handled within the application
public class AppException : Exception
{
    private readonly int httpStatusCode;
    public object LangOpts { get; set; } = null;
    public AppException() : base()
    {
        httpStatusCode = (int)HttpStatusCode.BadRequest;
    }
    public AppException(int httpStatusCode) : base()
    {
        this.httpStatusCode = httpStatusCode;
    }
    public AppException(HttpStatusCode httpStatusCode)
    {
        this.httpStatusCode = (int)httpStatusCode;
    }

    public AppException(string message) : base(message)
    {
        httpStatusCode = (int)HttpStatusCode.BadRequest;
    }
    public AppException(int httpStatusCode, string message) : base(message)
    {
        this.httpStatusCode = httpStatusCode;
    }
    public AppException(HttpStatusCode httpStatusCode, string message) : base(message)
    {
        this.httpStatusCode = (int)httpStatusCode;
    }
    public AppException(int httpStatusCode, string message, Exception inner) : base(message, inner)
    {
        this.httpStatusCode = httpStatusCode;
    }
    public AppException(HttpStatusCode httpStatusCode, string message, Exception inner) : base(message, inner)
    {
        this.httpStatusCode = (int)httpStatusCode;
    }
    public int StatusCode { get { return this.httpStatusCode; } }

    public AppException(string message, object langOpts = null)
    : base(message)
    {
        httpStatusCode = (int)HttpStatusCode.BadRequest;
        LangOpts = langOpts;
    }
    // public AppException(string message, params object[] args)
    //     : base(String.Format(CultureInfo.CurrentCulture, message, args))
    // {
    //     httpStatusCode = (int)HttpStatusCode.BadRequest;
    // }
}
