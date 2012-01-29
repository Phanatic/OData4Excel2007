// -----------------------------------------------------------------------
// <copyright file="ODataEntity.cs" company="Self">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ODataFeedClient.Objects
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.Edm;

    /// <summary>
    /// Generic object representing an Entity in an OData feed.
    /// </summary>
    public class ODataEntity
    {
        /// <summary>
        /// Initializes a new instance of the ODataEntity class.
        /// </summary>
        public ODataEntity()
        {
            this.NavigationProperties = new Dictionary<string, Uri>();
            this.Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets a Collection of navigation properties that are accessible through this instance.
        /// </summary>
        public Dictionary<string, Uri> NavigationProperties { get; private set; }

        /// <summary>
        /// Gets instance properties of this Entity.
        /// </summary>
        public Dictionary<string, object> Properties { get; private set; }

        /// <summary>
        /// Gets or sets link to send update requests for this entity to.
        /// </summary>
        public Uri EditLink { get; set; }

        /// <summary>
        /// Gets or sets the Link to send read requests for this entity to.
        /// </summary>
        public Uri ReadLink { get; set; }

        /// <summary>
        /// Gets or sets the Uri to access the media resource.
        /// </summary>
        public string MediaUri { get; set; }

        /// <summary>
        /// Gets or sets the type of the media resource.
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the instance type name of this entity.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the value of an instance property on this entity.
        /// </summary>
        /// <param name="propertyName">instance property name</param>
        /// <returns>Value of the instance property.</returns>
        public object this[string propertyName]
        {
            get
            {
                return this.Properties[propertyName];
            }

            set
            {
                this.Properties[propertyName] = value;
            }
        }
    }
}
