To create a new Migration execute the following command in the console:
	Add-Migration -Name Partner_Initial -Project "MicroServices\03 - Partner\SoftUnlimit.Cloud.Partner.Data" -StartupProject "MicroServices\03 - Partner\SoftUnlimit.Cloud.Partner.WebApi" -Context DbContextWrite -Verbose

To update the DataBase execute the following command in the console:
	Update-Database -Project "MicroServices\03 - Partner\SoftUnlimit.Cloud.Partner.Data" -StartupProject "MicroServices\03 - Partner\SoftUnlimit.Cloud.Partner.WebApi" -Context DbContextWrite -Verbose

To remove a migration (not runned) execute the following command in the console:
	Remove-Migration -Project "MicroServices\03 - Partner\SoftUnlimit.Cloud.Partner.Data" -StartupProject "MicroServices\03 - Partner\SoftUnlimit.Cloud.Partner.WebApi" -Context DbContextWrite -Verbose

To undo an already runned execute migration follow this steps:
	1.Update the DataBase to the previous migration
		Update-Database -Migration SoftUnlimit.Cloud.Partner.Data -Project SoftUnlimit.Cloud.Partner.Data -Project SoftUnlimit.Cloud.Partner.Data.WebApi -Context DbContextWrite -Verbose
	2. Remove the latest migrations:
		Remove-Migration -Project SoftUnlimit.Cloud.Partner.Data -Project SoftUnlimit.Cloud.Partner.Data.WebApi -Context DbContextWrite -Verbose
