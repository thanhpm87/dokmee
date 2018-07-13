using Services.AuthService.Models;
using System.Threading.Tasks;
using DokCapture.ServicenNetFramework.Auth.Models;

namespace DokCapture.ServicenNetFramework.Auth
{
  public interface IDokmeeService
  {
   // DokUser Login(string username, string password, ConnectorType type);

    Task<SignInResult> Login(string username, string password, ConnectorType type);

  }
}
