"use strict"

var connection = new signalR.HubConnectionBuilder().withUrl("http://localhost:5003/messageHub").build();

connection.on("ReceiveMessage", function (message) {
    // Вставка messageId, content и sendDate в список
    var messageHtml = '<li id="message_' + message.messageId + '">' +
        '<div class="messageId">Номер сообщения: ' + message.messageId + '</div>' +
        '<div class="messageText">Текст сообщения: ' + message.content + '</div>' +
        '<div class="messageDate">Дата отправки: ' + message.sendDate + '</div></li>';
    $('#messageList').append(messageHtml);
});

connection.start().then(function () {
    console.log("connect to web-socket!");
}).catch(function () {
    console.log('Faild to connect web socket');
})