const connection = new signalR.HubConnectionBuilder()
    .withUrl("/appointmentHub")
    .build();

connection.start().catch(err => console.error(err));

connection.on("AppointmentUpdated", function (appointmentId, status) {

    // Khi cập nhật status, reload cả 5 tab:
    reloadTab("#today", "/Doctor/ReloadToday");
    reloadTab("#upcoming", "/Doctor/ReloadUpcoming");
    reloadTab("#pending", "/Doctor/ReloadPending");
    reloadTab("#completed", "/Doctor/ReloadCompleted");
    reloadTab("#cancelled", "/Doctor/ReloadCancelled");
});

function reloadTab(containerSelector, url) {
    $.get(url, function (html) {
        $(containerSelector).html(html);
    });
}
