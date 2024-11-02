import fs from "fs/promises";
import path from "path";
import core from "@actions/core";

try {
	core.info("Starting upload action");

	const url = core.getInput("url");
	const apiKey = core.getInput("api-key");
	const repository = core.getInput("repository");
	const directory = core.getInput("directory");
	const extension = core.getInput("extension");

	const dirLength = directory.length + 1;

	const form = new FormData();
	form.append("api-key", apiKey);
	form.append("repository", repository);
	core.info(`Packing files in ${directory}:`);
	for (const entry of (await fs.readdir(directory, { recursive: true, withFileTypes: true })).filter(entry => entry.isFile() && (!extension || entry.name.endsWith(`.${extension}`)))) {
		const relative = path.join(entry.parentPath, entry.name).substring(dirLength).replaceAll("\\", "/");
		core.info(`  Reading ${relative}`);
		form.append(relative, new Blob([await fs.readFile(path.join(directory, relative))]));
	}
	core.info("Finished packing files.");

	core.info(`Calling ${url}`);
	const response = await fetch(url, { method: "POST", body: form });

	const chunks = [];
	for await (const chunk of response.body) {
		chunks.push(Buffer.from(chunk));
	}
	const message = Buffer.concat(chunks).toString("utf8");

	const fn = (response.status === 200 ? core.info : core.setFailed);
	fn(`  ${response.statusText} (${response.status})${(message !== "" ? `: ${message}` : "")}`);
} catch (error) {
	core.setFailed("Error during processing:");
	core.setFailed(error);
}
