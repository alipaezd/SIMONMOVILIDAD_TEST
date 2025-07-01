namespace Simon.Movilidad.Api.Helpers
{
    public static class Masking
    {
        public static string MaskDeviceCode(string code, int showStart = 3, int showEnd = 4, char maskChar = '*')
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length <= showStart + showEnd)
                return code;

            var start = code.Substring(0, showStart);
            var end = code.Substring(code.Length - showEnd);
            var masked = new string(maskChar, code.Length - showStart - showEnd);
            return start + masked + end;
        }
    }
}