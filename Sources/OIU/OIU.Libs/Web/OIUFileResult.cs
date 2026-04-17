namespace OIU.Libs.Web;


/// <summary>
/// Represents a file response payload, including its binary content, MIME type, and optional file name.
/// </summary>
/// <param name="Content">The file content as a byte array.</param>
/// <param name="ContentType">The MIME content type of the file (for example, <c>application/pdf</c>).</param>
/// <param name="Name">The optional file name to be used when presenting or downloading the file.</param>
public record OIUFileResult(byte[] Content, string ContentType, string? Name = null);