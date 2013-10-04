var mysql 	  = require('mysql');

var client = mysql.createConnection({
	host : 'localhost',
	user : 'root',
	password : '1234'
});

client.query('CREATE DATABASE IF NOT EXISTS gfxStatsDB', function(err) {
	console.log('DB gfxStatsDB NOTIF');
	if (err) {throw err;}
});
client.query('USE gfxStatsDB');

var sqlQuery = "CREATE TABLE IF NOT EXISTS gfxUserStatTable("+
			   "UserId INT unsigned default 1,"+
			   "UserName VARCHAR(20) not null,"+
			   "Wins INT unsigned default 0,"+
			   "Losses INT unsigned default 0,"+
			   "primary key(UserId)"+
			   ");";
client.query(sqlQuery, function(err) {
	console.log('Table gfxUserStatTable NOTIF');
	if (err) {throw err; }
});

exports.add_User = function(data, callback) {
 client.query("INSERT INTO gfxUserStatTable (UserId, UserName, Wins, Losses) values (?,?,?,?)", 
	[data.UserId, data.UserName, data.Wins, data.Losses], function(err, info) {
	callback(data.insertId);
	console.log('id: '+data.insertId);
	console.log('User '+data.UserName+' has '+data.Wins+' wins and '+data.Losses+' losses');
	});
}

exports.get_Users = function(callback) {
 client.query("SELECT * FROM gfxUserStatTable", function(err, results, fields) {
	callback(results);
	});
}