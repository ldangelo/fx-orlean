using Pulumi.Docker;

class MyStack : Stack
{
    public MyStack()
    {
        var appLabels = new InputMap<string> { { "app", "myapp" } };

        var eventServerDeployment = new Deployment(
            "eventserver-deployment",
            new DeploymentArgs
            {
                Spec = new DeploymentSpecArgs
                {
                    Selector = new LabelSelectorArgs { MatchLabels = appLabels },
                    Replicas = 1,
                    Template = new PodTemplateSpecArgs
                    {
                        Metadata = new ObjectMetaArgs { Labels = appLabels },
                        Spec = new PodSpecArgs
                        {
                            Containers =
                            {
                                new ContainerArgs
                                {
                                    Name = "eventserver",
                                    Image = "eventserver-image", // Replace with your image
                                    Ports = { new ContainerPortArgs { ContainerPortValue = 80 } },
                                },
                            },
                        },
                    },
                },
            }
        );

        var fxExpertDeployment = new Deployment(
            "fxexpert-deployment",
            new DeploymentArgs
            {
                Spec = new DeploymentSpecArgs
                {
                    Selector = new LabelSelectorArgs { MatchLabels = appLabels },
                    Replicas = 1,
                    Template = new PodTemplateSpecArgs
                    {
                        Metadata = new ObjectMetaArgs { Labels = appLabels },
                        Spec = new PodSpecArgs
                        {
                            Containers =
                            {
                                new ContainerArgs
                                {
                                    Name = "fxexpert",
                                    Image = "fxexpert-image", // Replace with your image
                                    Ports = { new ContainerPortArgs { ContainerPortValue = 80 } },
                                },
                            },
                        },
                    },
                },
            }
        );

        var eventServerService = new Service(
            "eventserver-service",
            new ServiceArgs
            {
                Spec = new ServiceSpecArgs
                {
                    Selector = appLabels,
                    Ports =
                    {
                        new ServicePortArgs { Port = 80, TargetPort = 80 },
                    },
                },
            }
        );

        var fxExpertService = new Service(
            "fxexpert-service",
            new ServiceArgs
            {
                Spec = new ServiceSpecArgs
                {
                    Selector = appLabels,
                    Ports =
                    {
                        new ServicePortArgs { Port = 80, TargetPort = 80 },
                    },
                },
            }
        );
        var eureka = new Container("eureka", new ContainerArgs
        {
            Image = "ldangelo/fortium-eureka:latest",
            Ports = { new ContainerPortArgs { Internal = 8761, External = 8761 } }
        });

        var postgres = new Container("postgres", new ContainerArgs
        {
            Image = "postgres:latest",
            Ports = { new ContainerPortArgs { Internal = 5432, External = 5432 } },
            Env = { "POSTGRES_PASSWORD=itsasecret" },
            Volumes = { new ContainerVolumeArgs { HostPath = "./docker/postgres/data", ContainerPath = "/var/lib/postgresql/data" } }
        });

        var keycloak = new Container("keycloak", new ContainerArgs
        {
            Image = "quay.io/keycloak/keycloak",
            Ports = { new ContainerPortArgs { Internal = 8080, External = 8085 } },
            Env = { "KEYCLOAK_ADMIN=admin", "KEYCLOAK_ADMIN_PASSWORD=itsasecret" },
            Volumes = { new ContainerVolumeArgs { HostPath = "./docker/keycloak", ContainerPath = "/opt/keycloak/data/import" } }
        });

        var zipkin = new Container("zipkin", new ContainerArgs
        {
            Image = "openzipkin/zipkin",
            Ports = { new ContainerPortArgs { Internal = 9411, External = 9411 } }
        });

        var prometheus = new Container("prometheus", new ContainerArgs
        {
            Image = "prom/prometheus:v2.54.1",
            Ports = { new ContainerPortArgs { Internal = 9090, External = 9091 } },
            Volumes = { new ContainerVolumeArgs { HostPath = "./docker/prometheus/prometheus.yml", ContainerPath = "/etc/prometheus/prometheus.yml" } }
        });

        var grafana = new Container("grafana", new ContainerArgs
        {
            Image = "grafana/grafana:10.4.10",
            Ports = { new ContainerPortArgs { Internal = 3000, External = 3000 } },
            Env = { "GF_AUTH_ANONYMOUS_ENABLED=true", "GF_AUTH_ANONYMOUS_ORG_ROLE=Admin", "GF_AUTH_DISABLE_LOGIN_FORM=true" },
            Volumes = { new ContainerVolumeArgs { HostPath = "./docker/grafana/grafana.ini", ContainerPath = "/etc/grafana/grafana.ini" } }
        });

        var tempo = new Container("tempo", new ContainerArgs
        {
            Image = "grafana/tempo:2.4.2",
            Ports = { new ContainerPortArgs { Internal = 3200, External = 3200 } },
            Volumes = { new ContainerVolumeArgs { HostPath = "./docker/grafana/tempo.yml", ContainerPath = "/etc/tempo.yml" } }
        });

        var loki = new Container("loki", new ContainerArgs
        {
            Image = "grafana/loki:3.1.2",
            Ports = { new ContainerPortArgs { Internal = 3100, External = 3100 } }
        });
}
