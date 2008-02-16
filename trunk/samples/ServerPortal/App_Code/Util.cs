using System;
using System.Web;
using DotNetOpenId;
using DotNetOpenId.Server;
using DotNetOpenId.Store;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Summary description for Util
/// </summary>
public class Util
{
    public static string ExtractUserName(Uri url)
    {        
        return url.Segments[url.Segments.Length - 1];
    }

    public static void GenerateHttpResponse(IEncodable response)
    {
        State.Session.Reset();
        WebResponse webresponse = null;
        Server server = new Server();
        try
        {
            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace("Preparing to send response");
            }
            #endregion
            
            webresponse = server.EncodeResponse(response);                        
        }
        catch (EncodingException e)
        {
            StringBuilder text = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in e.Response.EncodedFields)
                text.AppendLine(pair.Key + "=" + pair.Value);
            string error = @"
        <html><head><title>Error Processing Request</title></head><body>
        <p><pre>{0}</pre></p>
        <!--

        This is a large comment.  It exists to make this page larger.
        That is unfortunately necessary because of the 'smart'
        handling of pages returned with an error code in IE.

        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************
        *************************************************************

        --></body></html>";
            error = String.Format(error, HttpUtility.HtmlEncode(text.ToString()));
            HttpContext.Current.Response.StatusCode = 400;
            HttpContext.Current.Response.Write(error);
            HttpContext.Current.Response.Close();
        }

        if (((int)webresponse.Code) == 302)
        {
            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace(String.Format("Send response as 302 browser redirect to: '{0}'", webresponse.Headers["Location"]));
            }
            #endregion
            
            HttpContext.Current.Response.Redirect(webresponse.Headers["Location"]);
            return;
        }
        HttpContext.Current.Response.StatusCode = (int)webresponse.Code;
        foreach (string key in webresponse.Headers)
            HttpContext.Current.Response.AddHeader(key, webresponse.Headers[key]);

        if (webresponse.Body != null)
        {
            
            #region  Trace
            if (TraceUtil.Switch.TraceInfo)
            {
                TraceUtil.ServerTrace("Send response as server side HTTP response");                
            }
            
            if (TraceUtil.Switch.TraceVerbose)
            {
                TraceUtil.ServerTrace("HTTP Response headers follows:");
                TraceUtil.ServerTrace(webresponse.Headers);
                TraceUtil.ServerTrace("HTTP Response follows:");
                TraceUtil.ServerTrace(System.Text.Encoding.UTF8.GetString(webresponse.Body));
            }            
            #endregion            
            
            HttpContext.Current.Response.Write(System.Text.Encoding.UTF8.GetString(webresponse.Body));
            
        }
        // HttpContext.Current.Response.Flush();
        // HttpContext.Current.Response.Close();
    }
    
    public static  Uri ServerUri
    {
        get
        {
            UriBuilder builder = new UriBuilder(HttpContext.Current.Request.Url);
            builder.Path = HttpContext.Current.Response.ApplyAppPathModifier("~/server.aspx");
            builder.Query = null;
            builder.Fragment = null;
            return new Uri(builder.ToString(), true);
        }
    }

}
