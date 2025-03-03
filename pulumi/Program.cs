using Pulumi;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Apps.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Apps.V1;

class MyStack : Stack
{
    public MyStack()
    {
        var appLabels = new InputMap<string>
        {
            { "app", "myapp" }
        };

        var eventServerDeployment = new Deployment("eventserver-deployment", new DeploymentArgs
        {
            Spec = new DeploymentSpecArgs
            {
                Selector = new LabelSelectorArgs
                {
                    MatchLabels = appLabels
                },
                Replicas = 1,
                Template = new PodTemplateSpecArgs
                {
                    Metadata = new ObjectMetaArgs
                    {
                        Labels = appLabels
                    },
                    Spec = new PodSpecArgs
                    {
                        Containers = 
                        {
                            new ContainerArgs
                            {
                                Name = "eventserver",
                                Image = "eventserver-image", // Replace with your image
                                Ports = 
                                {
                                    new ContainerPortArgs
                                    {
                                        ContainerPortValue = 80
                                    }
                                }
                            }
                        }
                    }
                }
            }
        });

        var fxExpertDeployment = new Deployment("fxexpert-deployment", new DeploymentArgs
        {
            Spec = new DeploymentSpecArgs
            {
                Selector = new LabelSelectorArgs
                {
                    MatchLabels = appLabels
                },
                Replicas = 1,
                Template = new PodTemplateSpecArgs
                {
                    Metadata = new ObjectMetaArgs
                    {
                        Labels = appLabels
                    },
                    Spec = new PodSpecArgs
                    {
                        Containers = 
                        {
                            new ContainerArgs
                            {
                                Name = "fxexpert",
                                Image = "fxexpert-image", // Replace with your image
                                Ports = 
                                {
                                    new ContainerPortArgs
                                    {
                                        ContainerPortValue = 80
                                    }
                                }
                            }
                        }
                    }
                }
            }
        });

        var eventServerService = new Service("eventserver-service", new ServiceArgs
        {
            Spec = new ServiceSpecArgs
            {
                Selector = appLabels,
                Ports = 
                {
                    new ServicePortArgs
                    {
                        Port = 80,
                        TargetPort = 80
                    }
                }
            }
        });

        var fxExpertService = new Service("fxexpert-service", new ServiceArgs
        {
            Spec = new ServiceSpecArgs
            {
                Selector = appLabels,
                Ports = 
                {
                    new ServicePortArgs
                    {
                        Port = 80,
                        TargetPort = 80
                    }
                }
            }
        });
    }
}
using Pulumi;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Apps.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Apps.V1;

class MyStack : Stack
{
    public MyStack()
    {
        var appLabels = new InputMap<string>
        {
            { "app", "myapp" }
        };

        var eventServerDeployment = new Deployment("eventserver-deployment", new DeploymentArgs
        {
            Spec = new DeploymentSpecArgs
            {
                Selector = new LabelSelectorArgs
                {
                    MatchLabels = appLabels
                },
                Replicas = 1,
                Template = new PodTemplateSpecArgs
                {
                    Metadata = new ObjectMetaArgs
                    {
                        Labels = appLabels
                    },
                    Spec = new PodSpecArgs
                    {
                        Containers = 
                        {
                            new ContainerArgs
                            {
                                Name = "eventserver",
                                Image = "eventserver-image", // Replace with your image
                                Ports = 
                                {
                                    new ContainerPortArgs
                                    {
                                        ContainerPortValue = 80
                                    }
                                }
                            }
                        }
                    }
                }
            }
        });

        var fxExpertDeployment = new Deployment("fxexpert-deployment", new DeploymentArgs
        {
            Spec = new DeploymentSpecArgs
            {
                Selector = new LabelSelectorArgs
                {
                    MatchLabels = appLabels
                },
                Replicas = 1,
                Template = new PodTemplateSpecArgs
                {
                    Metadata = new ObjectMetaArgs
                    {
                        Labels = appLabels
                    },
                    Spec = new PodSpecArgs
                    {
                        Containers = 
                        {
                            new ContainerArgs
                            {
                                Name = "fxexpert",
                                Image = "fxexpert-image", // Replace with your image
                                Ports = 
                                {
                                    new ContainerPortArgs
                                    {
                                        ContainerPortValue = 80
                                    }
                                }
                            }
                        }
                    }
                }
            }
        });

        var eventServerService = new Service("eventserver-service", new ServiceArgs
        {
            Spec = new ServiceSpecArgs
            {
                Selector = appLabels,
                Ports = 
                {
                    new ServicePortArgs
                    {
                        Port = 80,
                        TargetPort = 80
                    }
                }
            }
        });

        var fxExpertService = new Service("fxexpert-service", new ServiceArgs
        {
            Spec = new ServiceSpecArgs
            {
                Selector = appLabels,
                Ports = 
                {
                    new ServicePortArgs
                    {
                        Port = 80,
                        TargetPort = 80
                    }
                }
            }
        });
    }
}
