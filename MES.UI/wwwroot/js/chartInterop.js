window.renderPieChart = function (canvasId, labels, data, colors) {
    var canvas = document.getElementById(canvasId);
    console.log(labels, data, colors);
    if (!canvas) {
        console.error("Canvas not found:", canvasId);
        return;
    }
    var ctx = canvas.getContext('2d');
    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors
            }]
        },
        options: {
            animation: false,
            responsive: false
        }
    });
};
