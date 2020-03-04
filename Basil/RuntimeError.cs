using System;

namespace BasilLang
{
    public class RuntimeError : Exception//RuntimeException
    {
        public readonly Token token;
        public readonly string message;

        public RuntimeError(Token token, string message) {
            //super(message);
            //base(message);
            this.message = message;
            this.token = token;
        }
    }
}
