using System;
using System.Collections.Generic;
using System.Linq;
using Amazon;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using DomainModel;
using PersistenceService;
using Attribute = Amazon.SimpleDB.Model.Attribute;

namespace SimpleDBProvider
{
    /// <summary>
    ///     AWS SimpleDB helper class.
    /// </summary>
    public class SimpleDBHelper : IDisposable
    {
        private bool _disposed;
        private string _domainPrefix;

        public SimpleDBHelper(string awsAccessKeyId, string awsSecretAccessKey, string domainPrefix)
        {
            _domainPrefix = domainPrefix;
            var awsConfig = new AmazonSimpleDBConfig
            {
                MaxErrorRetry = 10,

                // http://docs.aws.amazon.com/general/latest/gr/rande.html#sdb_region
                RegionEndpoint = RegionEndpoint.USEast1
            };
            Client = AWSClientFactory.CreateAmazonSimpleDBClient(awsAccessKeyId, awsSecretAccessKey, awsConfig);
        }

        public SimpleDBHelper(IAmazonSimpleDB amazonSimpleDBClient)
        {
            Client = amazonSimpleDBClient;
        }

        public IAmazonSimpleDB Client { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SimpleDBHelper()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Creates the SimpleDB Domain
        /// </summary>
        /// <param name="domainName"></param>
        public void CreateDomain(string domainName)
        {
            var request = new CreateDomainRequest { DomainName = _domainPrefix + domainName };

            Client.CreateDomain(request);
        }

        /// <summary>
        ///     Deletes the simpleDB Domain
        /// </summary>
        /// <param name="domainName"></param>
        public void DeleteDomain(string domainName)
        {
            var request = new DeleteDomainRequest { DomainName = _domainPrefix + domainName };

            Client.DeleteDomain(request);
        }

        /// <summary>
        ///     Puts attribiutes in SimpleDB
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="name"></param>
        /// <param name="replace"></param>
        /// <param name="value"></param>
        public void PutAttribute<T>(string itemName, string name, bool replace, string value) where T : DomainObject
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    value = "";

                var domain = _domainPrefix + typeof (T).FullName.Replace(".", "_");

                var replaceableAttribute = new ReplaceableAttribute
                {
                    Name = name,
                    Replace = replace,
                    Value = value
                };

                var request = new PutAttributesRequest(domain, IdentityMap.CreateKey<T>(itemName),
                    new List<ReplaceableAttribute> {replaceableAttribute});

                Client.PutAttributes(request);
            }
            catch (Exception ex)
            {
                // Log (ex.StackTrace);
            }
        }

        /// <summary>
        ///     Get a single attribute back from the item.
        /// </summary>
        /// <param name="domainName"></param>
        /// <param name="itemName"></param>
        /// <param name="name"></param>
        /// <returns>Returns the value of the attribute if it exists, otherwise an empty string.</returns>
        /// <remarks>Can't do multiple as no guarantee as to order.</remarks>
        public string GetAttribute<T>(string domainName, string itemName, string name) where T : DomainObject
        {
            var request = new GetAttributesRequest(_domainPrefix + domainName, IdentityMap.CreateKey<T>(itemName));

            var response = Client.GetAttributes(request);
            if (response.Attributes.Count > 0)
            {
                return response.Attributes[0].Value;
            }

            return string.Empty;
        }

        /// <summary>
        ///     Puts attribiutes in SimpleDB.
        /// </summary>
        public void PutAttributes<T>(string domainName, string itemName, List<ReplaceableAttribute> attributes) where T : DomainObject
        {
            var request = new PutAttributesRequest(_domainPrefix + domainName, IdentityMap.CreateKey<T>(itemName), attributes);
            Client.PutAttributes(request);
        }

        /// <summary>
        ///     Deletes the attributes.
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="names"></param>
        public void DeleteAttributes<T>(string domain, string itemName, IEnumerable<string> names) where T : DomainObject
        {
            try
            {
                var attributes = names.Select(name => new Attribute {Name = name}).ToList();
                var request = new DeleteAttributesRequest(_domainPrefix + domain, IdentityMap.CreateKey<T>(itemName), attributes);
                var deleteAttributesResponse = Client.DeleteAttributes(request);
            }
            catch (Exception ex)
            {
                // Log (ex.StackTrace);
            }
        }

        public bool VerifyDomainExists(string domain)
        {
            var listOfDomains = GetListOfDomains();
            if (listOfDomains.Contains(_domainPrefix + domain))
                return true;
            return false;
        }

        public List<string> GetListOfDomains()
        {
            var sdbListDomainsResponse = Client.ListDomains(new ListDomainsRequest());
            return sdbListDomainsResponse.DomainNames;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                if (!disposing)
                {
                    try
                    {
                        if (Client != null)
                        {
                            Client.Dispose();
                        }
                    }
                    finally
                    {
                        _disposed = true;
                    }
                }
            }
        }
    }
}