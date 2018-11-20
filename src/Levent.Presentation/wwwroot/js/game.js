"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

// Connection messages
connection.on("StartGame", function (message, turnOwner) {
    if (turnOwner === "turnOwner") {
        LetterGrid.style.display = "block";
        OpponentLetterGrid.style.display = "none";
    } else {
        LetterGrid.style.display = "none";
        OpponentLetterGrid.style.display = "none";
    }
    var li = document.createElement("li");
    li.textContent = message;
    document.getElementById("messagesList").appendChild(li);
});

connection.on("PlayOpponentLetter", function (message, opponentLetter, OpponentTurn) {
    document.getElementById("letter-opponent").innerText = opponentLetter;//////////////
    if (OpponentTurn === "OpponentTurn") {
        LetterGrid.style.display = "none";
        OpponentLetterGrid.style.display = "block";
    } else {
        LetterGrid.style.display = "none";
        OpponentLetterGrid.style.display = "none";
    }
    var li = document.createElement("li");
    li.textContent = message + " " + opponentLetter;
    document.getElementById("messagesList").appendChild(li);

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
    li.textContent = message;
    document.getElementById("messagesList").appendChild(li);

});

connection.on("GameIsOver", function (message, result) {
    alert(result.winnerUserName);
    document.getElementById("Grid").style.display = "none";
    document.getElementById("Letters").style.display = "none";
    document.getElementById("Messages").style.display = "none";
    document.getElementById("Result").style.display = "block";
    document.getElementById("Gameover").innerText = message;
});

connection.on("ResultUserName", function (winnerUserName, loserUserName) {
    document.getElementById("winnerName").innerText = winnerUserName;
    document.getElementById("loserName").innerText = loserUserName;
});

connection.on("ResultUserScore", function (winnerScore, loserScore) {
    document.getElementById("winnerScore").innerText = winnerScore;
    document.getElementById("loserScore").innerText = loserScore;
});

connection.on("ResultMeaningfulWords", function (winnerMeaningfulWords, loserMeaningfulWords) {
    for (var i = 0; i < winnerMeaningfulWords.length; i++) {
        var li = document.createElement("li");
        li.textContent = winnerMeaningfulWords[i];
        document.getElementById("winnerWords").appendChild(li);
    }
    for (var i = 0; i < loserMeaningfulWords.length; i++) {
        var li = document.createElement("li");
        li.textContent = loserMeaningfulWords[i];
        document.getElementById("loserWords").appendChild(li);
    }
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

// Game Methods
document.getElementById("joinGroupButton").addEventListener("click", function (event) {
    var x = document.getElementById("Join");
    if (x.style.display === "block") {
        x.style.display = "none";
    }
    var username = document.getElementById("userInput").value;
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
    ev.preventDefault();
    var letterCellId = ev.dataTransfer.getData("text");
    var letterElement = document.getElementById(letterCellId);
    var letter = letterElement.innerText;
    var xDimension = ev.target.attributes["x"].value;
    var yDimension = ev.target.attributes["y"].value;
    var copyElement = letterElement.cloneNode(true);
    copyElement.id = copyElement.id + xDimension + yDimension;
    copyElement.allowDrop = false;
    ev.target.appendChild(copyElement);


    if (letterCellId != "letter-opponent") {
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