"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

// WaitingOpponent
connection.on("WaitingOpponent", function (message) {
    var li = document.createElement("li");
    var list = document.getElementById("messagesList");
    li.textContent = message;
    list.insertBefore(li, list.childNodes[0]);
});

// Start Game
connection.on("StartGame", function (message, turnOwner, waitingUserName, playerName, LettersPoints) {

    for (var i in LettersPoints) {
        var tdPoint = document.createElement("td");
        tdPoint.align = "center";
        tdPoint.style = "vertical-align: middle; width: 50px; height: 50px;";
        var point = LettersPoints[i];
        tdPoint.innerText = point;
        document.getElementById("TurnOwnerLettersPoints").appendChild(tdPoint);

        var tdLetter = document.createElement("td");
        i = i.toUpperCase();
        tdLetter.innerText = i;
        tdLetter.id = "letter" + i;
        tdLetter.align = "center";
        tdLetter.style = "vertical-align: middle; width: 50px; height: 50px;";
        tdLetter.draggable = "true";
        document.getElementById("TurnOwnerLetters").appendChild(tdLetter);
        tdLetter.addEventListener('dragstart', function drag(ev) {
            console.log("drag ev");
            ev.dataTransfer.setData("text", ev.target.id);
        });
    }

    if (turnOwner === "turnOwner") {
        document.getElementById("userName").innerText = waitingUserName;
        LetterGrid.style.display = "block";
        OpponentLetterGrid.style.display = "none";
    } else {
        document.getElementById("userName").innerText = playerName;
        LetterGrid.style.display = "none";
        OpponentLetterGrid.style.display = "none";
    }
    var li = document.createElement("li");
    var list = document.getElementById("messagesList");
    li.textContent = message;
    list.insertBefore(li, list.childNodes[0]);
});

// Restart Game
connection.on("RestartGame", function (turnownerName, message) {
    document.getElementById("GridandMessage").style.display = "none";
    document.getElementById("Letters").style.display = "none";
    document.getElementById("RestartGame").style.display = "block";
    document.getElementById("Restart").innerText = turnownerName + "... " + message;
});

// Game Stream
connection.on("PlayOpponentLetter", function (message, opponentLetter, OpponentTurn) {
    document.getElementById("letter-opponent").innerText = opponentLetter;
    if (OpponentTurn === "OpponentTurn") {
        LetterGrid.style.display = "none";
        OpponentLetterGrid.style.display = "block";
    } else {
        LetterGrid.style.display = "none";
        OpponentLetterGrid.style.display = "none";
    }
    var li = document.createElement("li");
    var list = document.getElementById("messagesList");
    li.textContent = message;
    list.insertBefore(li, list.childNodes[0]);
});

connection.on("TurnOwnwer", function (message, TurnOwner) {
    if (TurnOwner === "TurnOwner") {
        LetterGrid.style.display = "block";
        OpponentLetterGrid.style.display = "none";
    } else {
        LetterGrid.style.display = "none";
        OpponentLetterGrid.style.display = "none";
    }
    var li = document.createElement("li");
    var list = document.getElementById("messagesList");
    li.textContent = message;
    list.insertBefore(li, list.childNodes[0]);
});

// Game is over
connection.on("GameIsOver", function (message, result) {
    document.getElementById("GridandMessage").style.display = "none";
    document.getElementById("Letters").style.display = "none";
    document.getElementById("Result").style.display = "block";
    document.getElementById("Gameover").innerText = message;

    document.getElementById("winnerName").innerText = result.winnerName;
    document.getElementById("loserName").innerText = result.loserName;

    document.getElementById("winnerScore").innerText = result.winnerScore;
    document.getElementById("loserScore").innerText = result.loserScore;

    for (var i = 0; i < result.winnerMeaningfulWords.length; i++) {
        var li1 = document.createElement("li");
        li1.textContent = result.winnerMeaningfulWords[i];
        document.getElementById("winnerWords").appendChild(li1);
    }
    for (var j = 0; j < result.loserMeaningfulWords.length; j++) {
        var li = document.createElement("li");
        li.textContent = result.loserMeaningfulWords[j];
        document.getElementById("loserWords").appendChild(li);
    }
});

// Exeption Messages
connection.on("GridCellException", function (message) {
    var li = document.createElement("li");
    var list = document.getElementById("messagesList");
    li.textContent = message;
    list.insertBefore(li, list.childNodes[0]);
});

connection.on("IncorrectLetterException", function (message) {
    var li = document.createElement("li");
    var list = document.getElementById("messagesList");
    li.textContent = message;
    list.insertBefore(li, list.childNodes[0]);
});

// Start
connection.start().catch(function (err) {
    return console.error(err.toString());
});

// Game Methods
document.getElementById("joinGame").addEventListener("click", function (event) {
    var x = document.getElementById("Join");
    if (x.style.display === "block") {
        x.style.display = "none";
    }
    var username = document.getElementById("userInput").value;
    username = username.toUpperCase();
    connection.invoke("JoinGroup", username).catch(function (err) {
        return console.error(err.toString());
    });
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
    var letterCellId = ev.dataTransfer.getData("text");
    var letterElement = document.getElementById(letterCellId);
    var letter = letterElement.innerText;
    var xDimension = ev.target.attributes["x"].value;
    var yDimension = ev.target.attributes["y"].value;
    var copyElement = letterElement.cloneNode(true);
    copyElement.id = copyElement.id + xDimension + yDimension;
    copyElement.allowDrop = false;
    copyElement.draggable = false;
    copyElement.drop = false;
    ev.target.appendChild(copyElement);

    copyElement.parentElement.allowDrop = false;
    copyElement.parentElement.draggable = false;
    copyElement.parentElement.drop = false;


    if (letterCellId !== "letter-opponent") {
        connection.invoke("Play", letter, xDimension, yDimension)
            .catch(function (err) {
                return console.error(err.toString());
            });
    } else {
        connection.invoke("PlayOpponent", xDimension, yDimension)
            .catch(function (err) {
                return console.error(err.toString());
            });
    }
    event.preventDefault();
}