// Option 1: Using POST with anti-forgery token (Recommended for MVC)
function deletePost(postId) {
    if (!confirm('ნამდვილად გსურთ ამ პოსტის წაშლა?')) {
        return;
    }

    // Get anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    const formData = new FormData();
    formData.append('id', postId);
    if (token) {
        formData.append('__RequestVerificationToken', token);
    }

    fetch('/BlogPost/Delete', {
        method: 'POST',
        body: formData
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                throw new Error('Failed to delete post');
            }
        })
        .then(data => {
            if (data.success) {
                displayMessage(data.message, 'success');
                // Remove the post card from DOM
                const postCard = document.querySelector(`[data-post-id="${postId}"]`)?.closest('.col-lg-4, .col-md-6');
                if (postCard) {
                    postCard.remove();
                }
            } else {
                displayMessage(data.message, 'danger');
            }
        })
        .catch(error => {
            displayMessage('შეცდომა მოხდა პოსტის წაშლისას', 'danger');
            console.error('Delete error:', error);
        });
}

// Option 2: Using DELETE method (if you prefer REST-style)
function deletePostREST(postId) {
    if (!confirm('ნამდვილად გსურთ ამ პოსტის წაშლა?')) {
        return;
    }

    fetch(`/BlogPost/Delete/${postId}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json',
        }
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                throw new Error('Failed to delete post');
            }
        })
        .then(data => {
            if (data.success) {
                displayMessage(data.message, 'success');
                const postCard = document.querySelector(`[data-post-id="${postId}"]`)?.closest('.col-lg-4, .col-md-6');
                if (postCard) {
                    postCard.remove();
                }
            } else {
                displayMessage(data.message, 'danger');
            }
        })
        .catch(error => {
            displayMessage('შეცდომა მოხდა პოსტის წაშლისას', 'danger');
            console.error('Delete error:', error);
        });
}

function displayMessage(message, type = 'success') {
    const messageDiv = document.createElement('div');
    messageDiv.className = `alert alert-${type} alert-dismissible fade show`;
    messageDiv.style.position = 'fixed';
    messageDiv.style.top = '20px';
    messageDiv.style.right = '20px';
    messageDiv.style.zIndex = '1050';
    messageDiv.style.minWidth = '300px';
    messageDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    document.body.appendChild(messageDiv);

    // Auto remove after 5 seconds
    setTimeout(() => {
        if (messageDiv && messageDiv.parentNode) {
            messageDiv.remove();
        }
    }, 5000);
}