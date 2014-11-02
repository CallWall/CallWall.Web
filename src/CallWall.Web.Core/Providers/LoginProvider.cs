using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CallWall.Web.Domain;

namespace CallWall.Web.Providers
{
    public sealed class LoginProvider : ILoginProvider
    {
        private readonly IUserRepository _userRepository;
        private readonly IEnumerable<IAccountAuthentication> _authenticationProviders;
        private readonly ILogger _logger;

        public LoginProvider(IUserRepository userRepository, ILoggerFactory loggerFactory, IEnumerable<IAccountAuthentication> authenticationProviders)
        {
            _userRepository = userRepository;
            _logger = loggerFactory.CreateLogger(GetType());
            _authenticationProviders = authenticationProviders;
        }

        public async Task<User> Login(string oAuthCode, string oAuthState)
        {
            _logger.Debug("Logging in...");   
            var account = CreateAccount(oAuthCode, oAuthState);
            _logger.Debug("Account created '{0}'", account.AccountId);
            //var user = await _userRepository.RegisterNewUser(account, Guid.NewGuid());
            var user = await _userRepository.Login(account);
            _logger.Debug("Logged in as userId '{0}' - {1}", user.Id, string.Join(",", user.Accounts.Select(a=>a.AccountId)));   
            return user;
        }

        public Task<User> GetUser(Guid userId)
        {
            return _userRepository.GetUserBy(userId);
        }

        private IAccount CreateAccount(string code, string state)
        {
            var authProvider = _authenticationProviders.Single(ap => ap.CanCreateAccountFromState(code, state));
            var account = authProvider.CreateAccountFromOAuthCallback(code, state);
            return account;
        }
    }
}