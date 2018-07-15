using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DokCapture.ServicenNetFramework.Auth;
using DokCapture.ServicenNetFramework.Auth.Models;
using Services.AuthService.Models;
using Dokmee.Dms.Connector.Advanced;
using Dokmee.Dms.Connector.Advanced.Core.Data;
using Dokmee.Dms.Connector.Advanced.Extension;
using Services.SessionHelperService;

namespace Services.AuthService
{
    public class DokmeeService : IDokmeeService
    {
        private ISessionHelperService _sessionHelperService;
        private DmsConnector _dmsConnector;
        private ConnectorModel _connectorModel;

        public DokmeeService(ISessionHelperService sessionHelperService)
        {
            _sessionHelperService = sessionHelperService;
        }

        private ConnectorModel ConnectorVm
        {
            get
            {
                if (_connectorModel == null)
                {
                    _connectorModel = new ConnectorModel();
                    ConnectorVm.SelectedConnectorType = ConnectorVm.ConnectorTypes.First();
                }
                return _connectorModel;
            }
        }
        
        public Task<SignInResult> Login(string username, string password, ConnectorType type)
        {
            DokUser user = new DokUser();
            SignInResult result = SignInResult.Success;

            if (_dmsConnector == null)
            {
                CreateConnector(username, password, type);
            }

            return Task.FromResult(result);
        }

        public IEnumerable<DokmeeCabinet> GetCurrentUserCabinet()
        {
            if (_dmsConnector == null)
            {
                var cabinets = CreateConnector(_sessionHelperService.Username, _sessionHelperService.Password, _sessionHelperService.ConnectorType);
                return cabinets.DokmeeCabinets;
            }

            var loginResult = _dmsConnector.Login(new LogonInfo
            {
                Username = _sessionHelperService.Username,
                Password = _sessionHelperService.Password
            });
            return loginResult.DokmeeCabinets;
        }

        private DokmeeCabinetResult CreateConnector(string username, string password, ConnectorType type)
        {
            username = username ?? _sessionHelperService.Username;
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new InvalideUsernameException("Username is null");
            }
            password = password ?? _sessionHelperService.Password;
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidePasswordException("Password is null");
            }
            //// initialize connector
            DokmeeApplication dApp = DokmeeApplication.DokmeeDMS;

            if (type == ConnectorType.DMS)
            {
                ConnectionInfo connInfo = new ConnectionInfo
                {
                    ServerName = ConnectorVm.Server,
                    UserID = "sa",
                    Password = "123456"
                };

                // register connection
                dApp = DokmeeApplication.DokmeeDMS;
                _dmsConnector = new DmsConnector(dApp);
                _dmsConnector.RegisterConnection<ConnectionInfo>(connInfo); // =>> this code cause lost session. Why???
            }
            else if (type == ConnectorType.WEB)
            {
                // register connection
                dApp = DokmeeApplication.DokmeeWeb;
                _dmsConnector = new DmsConnector(dApp);
                _dmsConnector.RegisterConnection<string>(ConnectorVm.HostUrl);
            }
            else if (type == ConnectorType.CLOUD)
            {
                // register connection
                dApp = DokmeeApplication.DokmeeCloud;
                _dmsConnector = new DmsConnector(dApp);
                _dmsConnector.RegisterConnection<string>("https://www.dokmeecloud.com");
            }

            var loginResult = _dmsConnector.Login(new LogonInfo
            {
                Username = username,
                Password = password
            });

            return loginResult;
        }
    }

    public class InvalideUsernameException : ArgumentException
    {
        public InvalideUsernameException(string mesage) : base(mesage)
        {

        }
    }

    public class InvalidePasswordException : ArgumentException
    {
        public InvalidePasswordException(string mesage) : base(mesage)
        {

        }
    }
}
