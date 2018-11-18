"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

// Connection messages
connection.on("StartGame", function (message, turnOwner) {
    if (turnOwner === "turnOwner") {
        LetterGrid.style.display = "block";
        OpponentLetterGrid.style.display == "none";
    } else {
        LetterGrid.style.display = "none";
        OpponentLetterGrid.style.display == "none";
    }
    var li = document.createElement("li");
    li.textContent = message;
    document.getElementById("messagesList").appendChild(li);
});

connection.on("PlayOpponentLetter", function (message, OpponentTurn) {
    if (OpponentTurn === "OpponentTurn") {
        LetterGrid.style.display = "none";
        OpponentLetterGrid.style.display == "block";
    } else {
        LetterGrid.style.display = "none";
        OpponentLetterGrid.style.display == "none";
    }
    var li = document.createElement("li");
    li.textContent = message;
    document.getElementById("messagesList").appendChild(li);
    
});

connection.on("TurnOwnwer", function (message) {
    if (turnOwner === "turnOwner") {
        LetterGrid.style.display = "block";
        OpponentLetterGrid.style.display == "none";
    } else {
        LetterGrid.style.display = "none";
        OpponentLetterGrid.style.display == "none";
    }
    var li = document.createElement("li");
    li.textContent = message;
    document.getElementById("messagesList").appendChild(li);
    
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

// Game Methods
document.getElementById("joinGroupButton").addEventListener("click", function (event) {
    var username = document.getElementById("userInput").value;
    connection.invoke("JoinGroup", username).catch(function (err) {
        return console.error(err.toString());
    });
    var x = document.getElementById("Join");
    if (x.style.display === "block") {
        x.style.display = "none";
    }
    var LetterGrid = document.getElementById("LetterGrid");
    if (LetterGrid.style.display === "none") {
        LetterGrid.style.display = "block";
    }
    event.preventDefault();
});

// Grid Show - Drag and Drop
function allowDrop(ev) {
    console.log("allowDrop ev");
    ev.preventDefault();
}

function drag(ev) {
    console.log("drag ev");
    ev.dataTransfer.setData("text", ev.target.id);
}

function drop(ev) {
    console.log("drop ev");
    ev.preventDefault();
    var letterCellId = ev.dataTransfer.getData("text");
    var letterElement = document.getElementById(letterCellId);
    var letter = letterElement.innerText;
    var xDimension = ev.target.attributes["x"].value;
    var yDimension = ev.target.attributes["y"].value;
    if (letterElement) {
        var copyElement = letterElement.cloneNode(true);
        letterElement.drag = function () { };
        letterElement.allowDrop = false;
        ev.target.appendChild(copyElement);
    }

    if (letterCellId != "letter-opponent") {
        connection.invoke("Play", letter, xDimension, yDimension)
            //
            .catch(function (err) {
            return console.error(err.toString());
        });
    } else {
        document.getElementById("letter-opponent").innerText = letter;
        connection.invoke("PlayOpponent", xDimension, yDimension)
            //
            .catch(function (err) {
            return console.error(err.toString());
        });
    }
    event.preventDefault();
}