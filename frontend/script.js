document.addEventListener("DOMContentLoaded", () => {
  const form = document.getElementById("visitorForm");
  const result = document.getElementById("result");

  form.addEventListener("submit", async (e) => {
    e.preventDefault();

    // Gather form data
    const data = {
      name: document.getElementById("name").value.trim(),
      email: document.getElementById("email").value.trim(),
      company: document.getElementById("company").value.trim(),
      purpose: document.getElementById("purpose").value.trim()
    };

    // Simple client-side validation
    if (!data.name || !data.email) {
      result.textContent = "⚠️ Please fill in required fields.";
      result.style.color = "red";
      return;
    }

    try {
      const response = await fetch("http://localhost:5215/api/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data)
      });

      if (response.ok) {
        const resJson = await response.json();
        result.textContent = "✅ " + resJson.message;
        result.style.color = "green";
        form.reset();
      } else {
        result.textContent = "❌ Something went wrong!";
        result.style.color = "red";
      }
    } catch (err) {
      console.error(err);
      result.textContent = "❌ Could not reach server.";
      result.style.color = "red";
    }
  });
});
