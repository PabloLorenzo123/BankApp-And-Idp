namespace Data
{
    public static class Queries
    {
        private static string GetParent(string dir) => Directory.GetParent(dir)?.FullName ?? string.Empty;
        private static string BaseDirectory => Path.Combine(
            GetParent(GetParent(GetParent(GetParent(Directory.GetCurrentDirectory())))), "Data");

        public static class IDP
        {
            private static readonly string BaseDir = BaseDirectory + "/IDP/Queries";

            public static readonly string FindOAuthClient = Path.Combine(BaseDir, "IDPFindOAuthClient.sql");

            public static readonly string QueryUserByUsername = Path.Combine(BaseDir, "QueryUserByUsername.sql");

            public static readonly string GetUserById = Path.Combine(BaseDir, "QueryUserById.sql");

            public static readonly string RegisterUser = Path.Combine(BaseDir, "RegisterUser.sql");

            public static readonly string GetUsers = Path.Combine(BaseDir, "GetAllUsers.sql");

            public static readonly string GetOAuthClientById = Path.Combine(BaseDir, "GetOAuthClient.sql");

            public static readonly string CreateOAuthClient = Path.Combine(BaseDir, "CreateOAuthClient.sql");

            public static readonly string GetAuthCode = Path.Combine(BaseDir, "GetAuthCode.sql");

            public static readonly string CreateAuthCode = Path.Combine(BaseDir, "CreateAuthorizationCode.sql");
        }

        public static class Bank
        {
            public static readonly string BaseDir = BaseDirectory + "/Bank/Queries";

            public static readonly string GetAccountById = Path.Combine(BaseDir, "GetAccountById.sql");
            public static readonly string GetBankAccountByUserId = Path.Combine(BaseDir, "GetBankAccountByUserId.sql");
            public static readonly string CreateBankAccount = Path.Combine(BaseDir, "CreateBankAccount.sql");
            public static readonly string GetBalance = Path.Combine(BaseDir, "GetBalance.sql");
            public static readonly string TransferMoney = Path.Combine(BaseDir, "TransferMoney.sql");
        }
    }
}
