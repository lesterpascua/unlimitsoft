To create a new Migration execute the following command in the console:
	Add-Migration -Name Name -Project UnlimitSoft.WebApi -StartupProject UnlimitSoft.WebApi -Context DbContextWrite -Verbose -OutputDir "Sources\Data\Migrations"

To update the DataBase execute the following command in the console:
	Update-Database -Project UnlimitSoft.WebApi -StartupProject UnlimitSoft.WebApi -Context DbContextWrite -Verbose -OutputDir "Sources\Data\Migrations"

To remove a migration (not runned) execute the following command in the console:
	Remove-Migration -Project UnlimitSoft.WebApi -StartupProject UnlimitSoft.WebApi -Context DbContextWrite -Verbose -OutputDir "Sources\Data\Migrations"

To undo an already runned execute migration follow this steps:
	1.Update the DataBase to the previous migration
		Update-Database -Migration Name -Project UnlimitSoft.WebApi -StartupProject UnlimitSoft.WebApi -Context DbContextWrite -Verbose -OutputDir "Sources\Data\Migrations"
	2. Remove the latest migrations:
		Remove-Migration -Project UnlimitSoft.WebApi -StartupProject UnlimitSoft.WebApi -Context DbContextWrite -Verbose -OutputDir "Sources\Data\Migrations"
