

# SENTINEL CACHE

*by Gennady Chernyak*

### Summary
This library allaws for a configurable auto starting cache system that can also be expired and renewed at will.
This caching system caches methods and not just objects as with the previous version. 

### Implementation Examples

#### Configuration

> Raw Request to Apiary
``` xml

<section name="sentinelCache" type="Viacom.MediaServices.WebApi.Utils.SentinelCacheConfigurations" allowLocation="true" allowDefinition="Everywhere" />


<sentinelCache>
  <functions>
    <function methodName="GetUsersAndGroups" assembly="my.assembly" initialAddDelay="1" name="my.namespace.users" temporaryBlockDuration=".5" updateInterval="2" maximumSurvival="5">
      <parameters>
        <![CDATA[userId=59331]]>
      </parameters>
    </function>
    <function methodName="GetConfig" assembly="my.assembly" initialAddDelay="1" name="my.namespace.configs" temporaryBlockDuration=".5" updateInterval="2" maximumSurvival="5">
      <parameters>
        <![CDATA[app=,key=,userId=59331]]>
      </parameters>
    </function>
    <!-- Object Parameter example -->
    <function methodName="GetAliasGroups" 
	assembly="my.assembly"
	name="my.namespace" initialAddDelay="1" temporaryBlockDuration=".5"
	updateInterval="1" maximumSurvival="1">
      <parameters>
        <![CDATA[<root>
		<Key>SomeParameter</Key>
		<UserRpc assembly="my.assembly" namespace="my.namespace.users">
			<Groups>
				<Members>
					<Description>Test</Description>
					<DomainMember>Member</DomainMember>
					<Group>Some Group</Group>
					<IsSecurityGroup>true</IsSecurityGroup>
				</Members>
			</Groups>
			<FirstName>Gennady</FirstName>
			<LastName>Chernyak</LastName>
		</UserRpc>
		</root>]]>
      </parameters>
    </function>
    <!-- End Example -->
  </functions>
</sentinelCache>
```
#### Hook

> Hook into some start event (ex: startup.cs or global.asax etc) This will read in our configured methods (below xml, supports json and lists)

``` c#
protected void Application_Start()
{		
	CacheConfigurations.CacheConfigurationFiles();
}
```
#### Reflections

> Reflect method and cache

``` c#
SentinelCache.GetGenericResults(() => method.Invoke(instance, objArray), method.Name,
                    TimeSpan.FromSeconds(config.InitialAddDelay), TimeSpan.FromMinutes(config.TemporaryBlockDuration),
                    TimeSpan.FromMinutes(config.UpdateInterval), TimeSpan.FromMinutes(config.MaximumCacheSurvival));
```

### Dependencies

- DotNet Core
- Newtonsoft.Json
- Microsoft.Extensions.Configuration (if applicable)

### Remarks
This is something I created in order to automatically start a cache that will expire itself and refresh. Initially it was used in a web application and then a web API.
Reason being is that IIS will automatically restart the application if the web.config gets changed. Even though this is not a recomeneded proceedure for a production enviroment
I thought it maybe a useful thing to have for testing.


