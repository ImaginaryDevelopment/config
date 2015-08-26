<Query Kind="FSharpProgram" />

//AdoHelper

open System.Collections.Generic
open System.Data
open System.Data.SqlClient

let openConnection (conn:SqlConnection) =
    if conn.State = ConnectionState.Closed then
        conn.Open()

let useConnection f conn = 
    openConnection conn
    f()

// 'a should never be IEnumerable
let inline get<'a> connectionString (withConnection:SqlConnection -> 'a) =
    use conn = new SqlConnection(connectionString)
    openConnection conn
    withConnection(conn)

// uncessary in F#
//let execute connectionString withConnection = 
//    get<Unit> connectionString withConnection

let inline inTrans f (conn:SqlConnection)=
        use trans = conn.BeginTransaction()
        let result = f(conn,trans)
        trans.Commit()
        result