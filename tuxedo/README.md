### tuxedo

Tuxedo is a database-agnostic SQL generator for DML. It also includes optional `IDbConnection` extension methods
that use Dapper underneath to provide `Insert`, `Update`, and `Delete` commands. Basically
it lets you use Dapper for query construction, but alleviates the tedium of `INSERT`, `UPDATE`, and `DELETE` calls
that are formulaic. It uses [TableDescriptor](http://github.com/danielcrenna/TableDescriptor) to perform the
identity mapping from objects to database tables.

#### I have to insert a lot of rows at a time, will this work?
No, not well. You should bulk insert large datasets, and you can use [Bulky](http://github.com/danielcrenna/Bulky) for that.

#### TODO
- Examples and documentation
- Map back all computed columns
- Batched inserts, deletes, and updates
