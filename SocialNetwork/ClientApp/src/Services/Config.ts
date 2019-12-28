interface Config {
    apiBase: string,
    identityBase: string
}
const config: Config = {
    apiBase: 'https://localhost:5001/API',
    identityBase: 'https://localhost:5001/Identity'
};
export default config;

export const ApplicationName = 'GSN';

export const QueryParameterNames = {
  ReturnUrl: 'returnUrl',
  Message: 'message'
};

export const LogoutActions = {
  LogoutCallback: 'logout-callback',
  Logout: 'logout',
  LoggedOut: 'logged-out'
};

export const LoginActions = {
  Login: 'login',
  LoginCallback: 'login-callback',
  LoginFailed: 'login-failed',
  Profile: 'profile',
  Register: 'register'
};

interface AuthConfig {
    DefaultLoginRedirectPath: string;
    ApiAuthorizationClientConfigurationUrl: string;
    ApiAuthorizationPrefix: string;
    Login: string;
    LoginFailed: string;
    LoginCallback: string;
    Register: string;
    Profile: string;
    LogOut: string;
    LoggedOut: string;
    LogOutCallback: string;
    IdentityRegisterPath: string;
    IdentityManagePath: string;
}

const prefix = '/authentication';

const authConfig: AuthConfig = {
    DefaultLoginRedirectPath: '/',
    ApiAuthorizationClientConfigurationUrl: `API/_configuration/${ApplicationName}`,
    ApiAuthorizationPrefix: prefix,
    Login: `${prefix}/${LoginActions.Login}`,
    LoginFailed: `${prefix}/${LoginActions.LoginFailed}`,
    LoginCallback: `${prefix}/${LoginActions.LoginCallback}`,
    Register: `${prefix}/${LoginActions.Register}`,
    Profile: `${prefix}/${LoginActions.Profile}`,
    LogOut: `${prefix}/${LogoutActions.Logout}`,
    LoggedOut: `${prefix}/${LogoutActions.LoggedOut}`,
    LogOutCallback: `${prefix}/${LogoutActions.LogoutCallback}`,
    IdentityRegisterPath: '/Identity/Account/Register',
    IdentityManagePath: '/Identity/Account/Manage'
};

export { authConfig };