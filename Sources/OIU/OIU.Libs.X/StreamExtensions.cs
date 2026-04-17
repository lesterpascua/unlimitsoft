namespace OIU.Libs;


/// <summary>
/// 
/// </summary>
public static class StreamExtensions
{
    extension (Stream @this)
    {
        /// <summary>
        /// Convert Stream to base64 string asynchronously.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async ValueTask<string> ToBase64Async(CancellationToken ct = default)
        {
            var raw = await @this.ToByteArrayAsync(ct);
            return Convert.ToBase64String(raw, Base64FormattingOptions.None);
        }
        /// <summary>
        /// Convert Stream to base64 string asynchronously.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async ValueTask<byte[]> ToByteArrayAsync(CancellationToken ct = default)
        {
            var position = @this.Position;
            var raw = new byte[@this.Length];
            if (await @this.ReadAsync(raw, ct) != raw.Length)
                throw new InvalidOperationException("Error in stream length");

            if (position == 0 && @this.CanSeek)
                @this.Seek(0, SeekOrigin.Begin);

            return raw;
        }
    }
}
