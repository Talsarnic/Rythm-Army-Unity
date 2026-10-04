const fs = require('fs');
const path = require('path');

const API_KEY = '3cef6c1a-0967-494d-bab5-4a831e4a45cf';
const MCP_URL = 'https://api.pixellab.ai/mcp';

async function callMcp(name, args) {
  const res = await fetch(MCP_URL, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${API_KEY}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      jsonrpc: '2.0',
      id: Date.now(),
      method: 'tools/call',
      params: {
        name: name,
        arguments: args
      }
    })
  });
  const text = await res.text();
  const dataLine = text.split('\n').find(l => l.startsWith('data: '));
  if (!dataLine) throw new Error('No data line returned: ' + text);
  return JSON.parse(dataLine.replace('data: ', ''));
}

async function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

async function extractDownload(text) {
  const match = text.match(/download:\s*(https:\/\/[^\s]+)/i);
  if (match) return match[1];
  try {
    const json = JSON.parse(text);
    return json.image_url || json.url || (json.images && json.images[0]?.url);
  } catch (e) {
    return null;
  }
}

async function extractJobId(text) {
  console.log('[DEBUG Raw Start Content]:', text);
  // Look for uuid pattern
  const match = text.match(/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i);
  if (match) return match[0];
  try {
    const json = JSON.parse(text);
    return json.job_id || json.background_job_id || json.id;
  } catch (e) {
    return null;
  }
}

async function generateAndSave(name, description, width, height, outFilePath) {
  console.log(`\n[Generating] ${name}: "${description}" (${width}x${height})...`);
  const startRes = await callMcp('create_image_pro_flash', {
    description: description,
    width: width,
    height: height,
    no_background: true
  });

  const content = startRes.result.content[0].text;
  const jobId = await extractJobId(content);
  if (!jobId) {
    throw new Error('Failed to find job ID in: ' + content);
  }
  console.log(`[Job Started] ID: ${jobId}`);

  while (true) {
    await sleep(4000);
    const pollRes = await callMcp('get_image', { job_id: jobId });
    const pollContent = pollRes.result.content[0].text;
    
    if (pollContent.includes('status: completed') || pollContent.includes('download:')) {
      const downloadUrl = await extractDownload(pollContent) || `https://api.pixellab.ai/mcp/images/${jobId}/download`;
      console.log(`[Downloading] ${downloadUrl}`);
      const imgRes = await fetch(downloadUrl, {
        headers: {
          'Authorization': `Bearer ${API_KEY}`
        }
      });
      const targetDir = path.dirname(outFilePath);
      if (!fs.existsSync(targetDir)) {
        fs.mkdirSync(targetDir, { recursive: true });
      }
      const buffer = Buffer.from(await imgRes.arrayBuffer());
      fs.writeFileSync(outFilePath, buffer);
      console.log(`[Saved] -> ${outFilePath} (${buffer.length} bytes)`);
      return outFilePath;
    } else if (pollContent.includes('status: failed')) {
      throw new Error('Generation failed: ' + pollContent);
    } else {
      const firstLine = pollContent.split('\n')[0];
      console.log(`[Status] ${firstLine}`);
    }
  }
}

async function run() {
  // Generate Banner
  await generateAndSave(
    'Banner',
    'Moonlighter style 32x32 pixel art standard bearer unit, fantasy RPG tribal warrior holding tall wooden flagpole with large vibrant golden silk flag cloth fluttering in the wind, red insignia on waving flag pennant, warm colors, side view',
    32,
    32,
    'Assets/Sprites/Units/pixellab_banner.png'
  );
}

run().catch(console.error);
