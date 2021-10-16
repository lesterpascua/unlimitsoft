To create a new Migration execute the following command in the console:
	Add-Migration -Name VirusScan_Initial -Project MicroServices\VirusScan\SoftUnlimit.Cloud.VirusScan.Data -StartupProject MicroServices\VirusScan\SoftUnlimit.Cloud.VirusScan.WebApi -Context DbContextWrite -Verbose

To update the DataBase execute the following command in the console:
	Update-Database -Project SoftUnlimit.Cloud.VirusScan.Data -Project SoftUnlimit.Cloud.VirusScan.Data.WebApi -Context DbContextWrite -Verbose

To remove a migration (not runned) execute the following command in the console:
	Remove-Migration -Project SoftUnlimit.Cloud.VirusScan.Data -Project SoftUnlimit.Cloud.VirusScan.Data.WebApi -Context DbContextWrite -Verbose

To undo an already runned execute migration follow this steps:
	1.Update the DataBase to the previous migration
		Update-Database -Migration SoftUnlimit.Cloud.VirusScan.Data -Project SoftUnlimit.Cloud.VirusScan.Data -Project SoftUnlimit.Cloud.VirusScan.Data.WebApi -Context DbContextWrite -Verbose
	2. Remove the latest migrations:
		Remove-Migration -Project SoftUnlimit.Cloud.VirusScan.Data -Project SoftUnlimit.Cloud.VirusScan.Data.WebApi -Context DbContextWrite -Verbose
