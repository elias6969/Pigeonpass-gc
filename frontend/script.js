const API_URL = "https://pigeonpass-backend-762376077749.europe-north1.run.app/api/register";

document.getElementById("visitor-form").addEventListener("submit", async (e) => {
  e.preventDefault();

  const data = {
    name: document.getElementById("name").value,
    email: document.getElementById("email").value,
    company: document.getElementById("company").value,
    purpose: document.getElementById("purpose").value,
  };

  try {
    const res = await fetch(API_URL, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });

    const result = await res.json();
    document.getElementById("message").innerText = result.message || "Error occurred!";
  } catch (err) {
    document.getElementById("message").innerText = "Network error!";
  }
});

