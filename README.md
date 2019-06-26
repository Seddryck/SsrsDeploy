<img src="https://github.com/Seddryck/RsPackage/raw/develop/RsPackage-title.png" width="160px">
# RsPackage

RsPackage is a tool to facilitate the automation of the deployment of reports, data sources and other artefacts to SQL Server Reporting Service (SSRS).

[![Build status](https://ci.appveyor.com/api/projects/status/7k5tda804jbcvlq4?svg=true)](https://ci.appveyor.com/project/Seddryck/RsPackage)
![Still maintained](https://img.shields.io/maintenance/yes/2018.svg)
[![nuget](https://img.shields.io/nuget/v/RsPackage.svg)](https://www.nuget.org/packages/RsPackage/)
[![nuget pre](https://img.shields.io/nuget/vpre/RsPackage.svg)](https://www.nuget.org/packages/RsPackage/)

#How-to

## Command line arguments

* ```-u``` or ```/url``` specifies the url of the target server for the deployment
* ```-f``` or ```/folder``` specifies the folder of the target server as the top-level folder for this deployment
* ```-s``` or ```/source``` specifies the path to the file containing the manifest for this deployment
* ```-r``` or ```/resources``` specifies the folder containing all the artefacts (rdl, rds and other files)
* ```-l``` or ```/logPath``` specifies the file where the log will be redirected. If missing logs are displayed in the console

## Elements in the manifest of deployment

* ```<Project>``` is the top element of the manifest
* ```<Folder>``` lets you define a sub-folder and its content
``` xml
<Folder Name="1">
   <Folder Name="1.1">
      <Folder Name="1.1.1">
      </Folder>
   </Folder>
   <Folder Name="1.2">
   </Folder>
</Folder>
<Folder Name="2">
   <Folder Name="2.1">
   </Folder>
</Folder>
```
* ```<Report>``` defines the name of the report and optionaly its filename (Element ```Path```), description (Element ```Description```) and visibility (attribute ```Hidden```)
``` xml
<Folder Name="1">
   <Report Name="My first report" Hidden="True">
      <Description>This is my first report to be published</Description>
      <Path>My1stReport.rdl</Path>
	  <DateSetMap>
        <DataSet Name="DataSetA" Reference="DataSet1" />
        <DataSet Name="DataSetB" Reference="DataSet2" />
      </DateSetMap>
   </Report>
   <Report Name="My second report"/>
</Folder>
```
* ```<DataSource>``` defines the name of data source and optionaly its filename (Element ```Path```), and  preserve (attribute ```overwrite```)
``` xml
<DataSource Name="My data source" Overwrite="False">
  <Path>MyDS.rds</Path>
</DataSource>
```
* ```<DataSet>``` defines the name of Shared Dataset and optionaly its filename (Element ```Path```)
``` xml
<DataSet Name="My datasets" Overwrite="False" DataSource="My data source">
  <Path>MyDS.rsd<Path>
</DataSet>
```
* ```<Membership>``` defines the overload of a ```Role``` for the specific catalog item (and children). Additional members in the overloaded ```Role``` are specified in ```<Member>``` elements.
``` xml
<Folder>
   <Membership Role="Browser">
      <Member>Domain\User1</Member>
      <Member>Domain\User2</Member>
    </Membership>
</Folder>
```
