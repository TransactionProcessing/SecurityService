namespace SecurityService.BusinessLogic;

using System;

public class KeycloakOptions
{
    public String AdminClientId { get; set; }

    public String AdminClientSecret { get; set; }

    public String AdminRealm { get; set; } = "master";

    public String Realm { get; set; }

    public String IssuerUrl { get; set; }

    public String ServerUrl { get; set; }
}
