var db_helper = require("./db_helper.js"),
	app 	  = require('http').createServer(handler),
	fs		  = require('fs'),
	io 		  = require('socket.io').listen(app);
	
app.listen(8000);

function handler ( req, res ) {
	fs.readFile( __dirname + '/index.html' , function ( err, data ) {
		if ( err ) {
			console.log( err );
			res.writeHead(500);
			return res.end( 'Error retrieving index.html' );
		}
		res.writeHead(200);
		res.end( data );
	});
}

io.sockets.on('connection', function(client) {
	console.log('a client has connected');
	
	db_helper.get_Users(function(users) {
		client.emit('populate', users);
	});
	
	client.on('add user', function(data) {
		db_helper.add_User(data, function(UserId) {
			db_helper.get_Users(function(users) {
				client.emit('populate', users);
			});
		});
	});
});
	
/*
connection.end(function(err) {
	// terminated
});
*/