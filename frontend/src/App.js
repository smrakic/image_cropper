import { useState } from 'react';
import Cropper from 'react-easy-crop';
import './App.css';

function App() {
  const [image, setImage] = useState(null);
  const [imageFile, setImageFile] = useState(null);
  const [crop, setCrop] = useState({ x: 0, y: 0 });
  const [zoom, setZoom] = useState(1);
  const [croppedAreaPixels, setCroppedAreaPixels] = useState(null);
  const [preview, setPreview] = useState(null);
  const [loading, setLoading] = useState(false);

  // Config states
  const [scaleDown, setScaleDown] = useState(0.1);
  const [logoPosition, setLogoPosition] = useState('bottom-right');
  const [logoImage, setLogoImage] = useState(null);
  const [savedConfigId, setSavedConfigId] = useState(null);

  const handleImageUpload = (e) => {
    const file = e.target.files[0];
    if (file) {
      setImageFile(file);
      const reader = new FileReader();
      reader.onload = (event) => {
        setImage(event.target.result);
      };
      reader.readAsDataURL(file);
    }
  };

  const onCropComplete = (croppedArea, croppedAreaPixels) => {
    setCroppedAreaPixels(croppedAreaPixels);
  };

  const handleSaveConfig = async () => {
    const formData = new FormData();
    formData.append('scaleDown', scaleDown);
    formData.append('logoPosition', logoPosition);
    if (logoImage) formData.append('logoImage', logoImage);

    const url = savedConfigId
      ? `http://localhost:5000/api/config/${savedConfigId}`
      : 'http://localhost:5000/api/config';
    const method = savedConfigId ? 'PUT' : 'POST';

    const response = await fetch(url, { method, body: formData });
    if (response.ok) {
      const data = await response.json();
      setSavedConfigId(data.id);
      alert('Config sačuvan!');
    } else {
      alert('Greška pri čuvanju configa!');
    }
  };

  const handlePreview = async () => {
    if (!croppedAreaPixels || !imageFile) {
      alert('Prvo odaberi crop area!');
      return;
    }
    setLoading(true);
    try {
      const formData = new FormData();
      formData.append('imageData', imageFile);
      formData.append('x', Math.round(croppedAreaPixels.x));
      formData.append('y', Math.round(croppedAreaPixels.y));
      formData.append('width', Math.round(croppedAreaPixels.width));
      formData.append('height', Math.round(croppedAreaPixels.height));

      const response = await fetch('http://localhost:5000/api/image/preview', {
        method: 'POST',
        body: formData
      });

      if (response.ok) {
        const blobResponse = await response.blob();
        const url = URL.createObjectURL(blobResponse);
        setPreview(url);
        alert('Preview uspješno!');
      } else {
        const error = await response.text();
        alert('Greška: ' + response.status + '\n' + error);
      }
    } catch (error) {
      alert('Greška pri preview-u: ' + error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleGenerate = async () => {
    if (!croppedAreaPixels || !imageFile) {
      alert('Prvo odaberi crop area!');
      return;
    }
    setLoading(true);
    try {
      const formData = new FormData();
      formData.append('imageData', imageFile);
      formData.append('x', Math.round(croppedAreaPixels.x));
      formData.append('y', Math.round(croppedAreaPixels.y));
      formData.append('width', Math.round(croppedAreaPixels.width));
      formData.append('height', Math.round(croppedAreaPixels.height));

      const response = await fetch('http://localhost:5000/api/image/generate', {
        method: 'POST',
        body: formData
      });

      if (response.ok) {
        const blobResponse = await response.blob();
        const url = URL.createObjectURL(blobResponse);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'cropped-image.png';
        link.click();
        alert('Slika downloadana!');
      } else {
        const error = await response.text();
        alert('Greška: ' + response.status + '\n' + error);
      }
    } catch (error) {
      alert('Greška pri generiranju: ' + error.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="App">
      <h1>Image Cropper</h1>

      {/* Config sekcija */}
      <div className="config-section">
        <h2>Logo Konfiguracija</h2>
        <div>
          <label>Logo slika: </label>
          <input
            type="file"
            accept="image/png"
            onChange={(e) => setLogoImage(e.target.files[0])}
          />
        </div>
        <div>
          <label>Pozicija loga: </label>
          <select value={logoPosition} onChange={(e) => setLogoPosition(e.target.value)}>
            <option value="top-left">Gore lijevo</option>
            <option value="top-right">Gore desno</option>
            <option value="bottom-left">Dolje lijevo</option>
            <option value="bottom-right">Dolje desno</option>
            <option value="center">Centar</option>
          </select>
        </div>
        <div>
          <label>Veličina loga (max 0.25): {scaleDown}</label>
          <input
            type="range"
            min={0.01}
            max={0.25}
            step={0.01}
            value={scaleDown}
            onChange={(e) => setScaleDown(parseFloat(e.target.value))}
          />
        </div>
        <button onClick={handleSaveConfig}>Sačuvaj Config</button>
      </div>

      {/* Upload sekcija */}
      <div className="upload-section">
        <label htmlFor="file-input">Odaberi PNG sliku:</label>
        <input
          id="file-input"
          type="file"
          accept="image/png"
          onChange={handleImageUpload}
        />
      </div>

      {image && (
        <div>
          <div className="crop-container">
            <Cropper
              image={image}
              crop={crop}
              zoom={zoom}
              aspect={4 / 3}
              onCropChange={setCrop}
              onCropComplete={onCropComplete}
              onZoomChange={setZoom}
            />
          </div>

          <div className="controls">
            <div className="control-group">
              <label htmlFor="zoom-slider">Zoom:</label>
              <input
                id="zoom-slider"
                type="range"
                min={1}
                max={3}
                step={0.1}
                value={zoom}
                onChange={(e) => setZoom(parseFloat(e.target.value))}
              />
              <span>{zoom.toFixed(2)}x</span>
            </div>

            <button onClick={handlePreview} className="btn-preview" disabled={loading}>
              {loading ? 'Učitavanje...' : 'Preview'}
            </button>

            <button onClick={handleGenerate} className="btn-generate" disabled={loading}>
              {loading ? 'Učitavanje...' : ' Generate & Download'}
            </button>
          </div>

          {preview && (
            <div className="preview-container">
              <h2>Preview (5% scaled):</h2>
              <img src={preview} alt="preview" />
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default App;