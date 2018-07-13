using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DokCapture.ServicenNetFramework.Auth;
using DokCapture.ServicenNetFramework.Auth.Models;
using Services.AuthService.Models;
using Dokmee.Dms.Connector.Advanced;
using Dokmee.Dms.Connector.Advanced.Core.Data;
using Dokmee.Dms.Connector.Advanced.Extension;

namespace Services.AuthService
{
  public class DokmeeService : IDokmeeService
  {
    private DmsConnector _dmsConnector;
    private ConnectorModel _connectorModel;
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

      //ConnectorVm.IsProgressVisible = Visibility.Visible;

      //// initialize connector
      DokmeeApplication dApp = DokmeeApplication.DokmeeDMS;

      if (type == ConnectorType.DMS)
      {
        ConnectionInfo connInfo = new ConnectionInfo();
        connInfo.ServerName = ConnectorVm.Server;
        connInfo.UserID = "sa";
        connInfo.Password = "123456";

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

      var cabinets = _dmsConnector.Login(new LogonInfo { Username = username, Password = password });
      var resutls = cabinets.DokmeeCabinets.Select(x => new Cabinet { Name = x.CabinetName, ID = x.CabinetID }) as IEnumerable<Cabinet>;

      return Task.FromResult(result);
    }
  }
}
