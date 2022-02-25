To create a new Migration execute the following command in the console:
	Add-Migration -Name Name -Project SoftUnlimit.WebApi -StartupProject SoftUnlimit.WebApi -Context DbContextWrite -Verbose -OutputDir "Sources\Data\Migrations"

To update the DataBase execute the following command in the console:
	Update-Database -Project SoftUnlimit.WebApi -StartupProject SoftUnlimit.WebApi -Context DbContextWrite -Verbose -OutputDir "Sources\Data\Migrations"

To remove a migration (not runned) execute the following command in the console:
	Remove-Migration -Project SoftUnlimit.WebApi -StartupProject SoftUnlimit.WebApi -Context DbContextWrite -Verbose -OutputDir "Sources\Data\Migrations"

To undo an already runned execute migration follow this steps:
	1.Update the DataBase to the previous migration
		Update-Database -Migration Name -Project SoftUnlimit.WebApi -StartupProject SoftUnlimit.WebApi -Context DbContextWrite -Verbose -OutputDir "Sources\Data\Migrations"
	2. Remove the latest migrations:
		Remove-Migration -Project SoftUnlimit.WebApi -StartupProject SoftUnlimit.WebApi -Context DbContextWrite -Verbose -OutputDir "Sources\Data\Migrations"
