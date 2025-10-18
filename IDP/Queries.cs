namespace Memento.Data
{
    public static class Queries
    {
        private const string baseDirectory = "Data/Queries";

        public static class IDPQueries
        {
            private const string baseDir = baseDirectory + "/IDP";

            public static readonly string FindOAuthClient = Path.Combine(
                Path.Combine(Directory.GetCurrentDirectory(), baseDir), "IDPFindOAuthClient.sql"
            );

            public static readonly string QueryUserByUsername = Path.Combine(
                Path.Combine(Directory.GetCurrentDirectory(), baseDir), "QueryUserByUsername.sql"
            );
        }

    }
}
