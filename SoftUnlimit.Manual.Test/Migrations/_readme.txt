Update-Database -Project Own.CQRS.Test -Context TestDbContext -Verbose
Add-Migration -Name Initial -Project Own.CQRS.Test -Context TestDbContext -Verbose
Remove-Migration -Project TurOP.Contract.Data -Context ContractWrite -Verbose
