
export function setFormEnterSubmit(bc) {
    var backendContext = bc;

    // Get the textarea element by ID
    const promptElement = document.getElementById('prompt');

    // Add a 'keypress' event
    promptElement.addEventListener('keypress', function (event) {
        // Verify if is pressed enter
        if (event.key === 'Enter') {
            // get textarea element and trim it
            promptElement.value.trim();
            var question = promptElement.value;

            // Call the method on back-end of this page.
            backendContext.invokeMethodAsync("callSendQuestion", question);
        }
    });
}