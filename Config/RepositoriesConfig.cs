using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.XmlUtility;

namespace OsmImportToSqlServer.Config
{
    public class RepositoriesConfig
    {
        public RepositoriesConfig(XElement repositoriesSection)
        {
            if (XmlUtility.IsExistElementsInXElement(repositoriesSection))
            {
                foreach (XElement repositoriyConfig in repositoriesSection.Elements())
                {
                    switch (repositoriyConfig.Name.ToString())
                    {
                        case "RelationRolesRepository":
                            this.ProcessSectionRelationRoles(repositoriyConfig);
                            break;
                        case "TagsValueRepository":
                            this.ProcessSectionTagsValue(repositoriyConfig);
                            break;
                    }
                }
            }
            else
            {
                throw new XmlException(@"Element <repositories> not found or does not contain elements");
            }
        }

        public void ProcessSectionRelationRoles(XElement sectionRelationRoles)
        {
            if (XmlUtility.IsExistAttributesInXElement(sectionRelationRoles))
            {
                string connString = "", typeProvider = "";
                foreach (XAttribute xAttribute in sectionRelationRoles.Attributes())
                {

                    switch (xAttribute.Name.ToString())
                    {
                        case "connString":
                            connString = xAttribute.Value;
                            break;
                        case "typeRepository":
                            typeProvider = xAttribute.Value;
                            break;
                    }
                }

                if (!String.IsNullOrEmpty(connString) && !String.IsNullOrEmpty(typeProvider))
                {
                    this.RelationRoles = new RelationRolesRepositoryConfig(typeProvider, connString);
                }
                else
                {
                    throw new XmlException("Attributes connString and typeRepository not found in section <repositories>");
                }

            }
        }

        public void ProcessSectionTagsValue(XElement sectionTagsValue)
        {
            if (XmlUtility.IsExistAttributesInXElement(sectionTagsValue))
            {
                string connString = "", typeProvider = "";
                foreach (XAttribute xAttribute in sectionTagsValue.Attributes())
                {

                    switch (xAttribute.Name.ToString())
                    {
                        case "connString":
                            connString = xAttribute.Value;
                            break;
                        case "typeRepository":
                            typeProvider = xAttribute.Value;
                            break;
                    }
                }

                if (!String.IsNullOrEmpty(connString) && !String.IsNullOrEmpty(typeProvider))
                {
                    this.TagsValue = new TagsValueRepositoryConfig(typeProvider, connString);
                }
                else
                {
                    throw new XmlException("Attributes connString and typeRepository not found in section <repositories>");
                }

            }
        }

        public RelationRolesRepositoryConfig RelationRoles { get; set; }
        public TagsValueRepositoryConfig TagsValue { get; set; }
    }

    public class TagsValueRepositoryConfig : RepositoryConfig
    {
        public TagsValueRepositoryConfig(string providerRepositoryType, string connectionString)
            : base (providerRepositoryType, connectionString){}
    }

    public class RelationRolesRepositoryConfig : RepositoryConfig
    {
        public RelationRolesRepositoryConfig(string providerRepositoryType, string connectionString)
            : base (providerRepositoryType, connectionString){}
    }

    public abstract class RepositoryConfig
    {
        protected RepositoryConfig(string providerRepositoryType, string connectionString)
        {
            this._type = providerRepositoryType;
            this.ConnectionString = connectionString;
        }

        public virtual Type TypeRepository
        {
            get { return Type.GetType(_type); }
        }

        public string ConnectionString { get; set; }

        private readonly string _type;
    }
}
