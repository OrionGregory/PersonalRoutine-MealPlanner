document.addEventListener('DOMContentLoaded', function () {
    const draggables = document.querySelectorAll('.draggable');
    const droppables = document.querySelectorAll('.droppable');

    let draggedElement = null;

    draggables.forEach(draggable => {
        draggable.addEventListener('dragstart', function (e) {
            draggedElement = e.target;
        });

        draggable.addEventListener('dragend', function () {
            draggedElement = null;
        });
    });

    droppables.forEach(droppable => {
        droppable.addEventListener('dragover', function (e) {
            e.preventDefault();
        });

        droppable.addEventListener('drop', function (e) {
            e.preventDefault();

            const targetDay = e.currentTarget.dataset.day;
            const draggedRoutineId = draggedElement.dataset.routineId;

            if (draggedRoutineId && targetDay) {
                const sourceDay = draggedElement.closest('tr').dataset.day;

                if (sourceDay !== targetDay) {
                    // Send swap request to the server
                    fetch('/Person/SwapRoutines', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]').value
                        },
                        body: JSON.stringify({
                            routineId: draggedRoutineId,
                            targetDay: targetDay
                        })
                    })
                        .then(response => {
                            if (response.ok) {
                                location.reload(); // Reload the page to reflect changes
                            } else {
                                response.json().then(error => {
                                    alert(error.message || 'Error swapping routines. Please try again.');
                                });
                            }
                        })
                        .catch(error => {
                            console.error('Error:', error);
                            alert('Error swapping routines. Please try again.');
                        });

                }
            }
        });
    });
});
