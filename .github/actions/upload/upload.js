import fs from "fs/promises";
import path from "path";
import core from "@actions/core";

try {
	const url = core.getInput("url");
	const apiKey = core.getInput("api-key");
	const repository = core.getInput("repository");
	const directory = core.getInput("directory");
	const extension = core.getInput("extension");

	const form = new FormData();
	form.append("api-key", apiKey);
	form.append("repository", repository);
	for (const relative of (await fs.readdir(directory)).filter(relative => !extension || relative.endsWith(`.${extension}`))) {
		form.append(relative, new Blob([await fs.readFile(path.join(directory, relative))]));
	}

	const response = await fetch(url, { method: "POST", body: form });

	const chunks = [];
	for await (const chunk of response.body) {
		chunks.push(Buffer.from(chunk));
	}
	const message = Buffer.concat(chunks).toString("utf8");

	const fn = (response.statusCode === 200 ? core.info : core.setFailed);
	fn(`${response.statusText} (${response.status})${(message !== "" ? `: ${message}` : "")}`);
} catch (error) {
	core.setFailed(`Error during processing: ${error}`);
}
