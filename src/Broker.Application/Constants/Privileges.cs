namespace Broker.Application.Constants
{
    public static class Privileges
    {
        public static class Broker
        {
            public const string ENTITY_NAME = "broker";

            public static class Rights
            {
                public const string VIEW_BROKER = "read_broker";
                public const string CREATE_BROKER = "write_broker";
                public const string EDIT_BROKER = "write_broker";
                public const string DELETE_BROKER = "delete_broker";
            }
            public static class FullRights
            {
                public const string READ_BROKER = "a0f1c338-1eff-40ff-997e-64f08e141b06/broker/read_broker";
                public const string CREATE_BROKER = "a0f1c338-1eff-40ff-997e-64f08e141b06/broker/write_broker";
                public const string EDIT_BROKER = "a0f1c338-1eff-40ff-997e-64f08e141b06/broker/write_broker";
                public const string DELETE_BROKER = "a0f1c338-1eff-40ff-997e-64f08e141b06/broker/delete_broker";
            }
        }
        public static class Integrations
        {
            public const string ENTITY_NAME = "integration";
            public static class Rights
            {
                public const string VIEW_INTEGRATION = "read_integration";
                public const string CREATE_INTEGRATION = "write_integration";
                public const string EDIT_INTEGRATION = "write_integration";
                public const string DELETE_INTEGRATION = "delete_integration";
            }
            public static class FullRights
            {
                public const string READ_INTEGRATION = "a0f1c338-1eff-40ff-997e-64f08e141b06/integration/read_integration";
                public const string VIEW_INTEGRATION = "a0f1c338-1eff-40ff-997e-64f08e141b06/integration/read_integration";
                public const string CREATE_INTEGRATION = "a0f1c338-1eff-40ff-997e-64f08e141b06/integration/write_integration";
                public const string EDIT_INTEGRATION = "a0f1c338-1eff-40ff-997e-64f08e141b06/integration/write_integration";
                public const string DELETE_INTEGRATION = "a0f1c338-1eff-40ff-997e-64f08e141b06/integration/delete_integration";
            }
        }
        public static class Device
        {
            public static class FullRights
            {
                public const string WRITE_DEVICE = "entities/device/objects/*/privileges/write_device";
            }
        }
        public static class Asset
        {
            public static class FullRights
            {
                public const string WRITE_ASSET = "entities/asset/objects/*/privileges/write_asset";
                public const string READ_ASSET = "entities/asset/objects/*/privileges/read_asset";
            }
        }
        public static class AssetTemplate
        {
            public static class FullRights
            {
                public const string WRITE_ASSET_TEMPLATE = "entities/asset_template/objects/*/privileges/write_asset_template";
                public const string READ_ASSET_TEMPLATE = "entities/asset_template/objects/*/privileges/read_asset_template";
            }
        }

        public static class Configuration
        {
            public const string ENTITY_NAME = "asset_configuration";
            public static class Rights
            {
                public const string SHARE_CONFIGURATION = "share_asset_configuration";

            }

            public static class FullRights
            {
                public const string SHARE_CONFIGURATION = "a0f1c338-1eff-40ff-997e-64f08e141b06/asset_configuration/share_asset_configuration";
            }
        }
    }
}
