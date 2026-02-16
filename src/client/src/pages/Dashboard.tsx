import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import api from '../api/api';
import { useAuth } from '../context/AuthContext';
import { useMutation } from '@tanstack/react-query';

const encryptSchema = z.object({
  file: z.any().refine((files) => files?.length === 1, "File is required"),
  keySize: z.enum(["128", "192", "256"]),
  uploadToDrive: z.boolean().optional(),
  shareToList: z.string().optional(),
});

const decryptSchema = z.object({
  file: z.any().refine((files) => files?.length === 1, "File is required"),
});

type EncryptForm = z.infer<typeof encryptSchema>;
type DecryptForm = z.infer<typeof decryptSchema>;

export default function Dashboard() {
  const { user, logout } = useAuth();

  // Encrypt Form
  const { register: registerEncrypt, handleSubmit: handleSubmitEncrypt, formState: { errors: errorsEncrypt }, watch } = useForm<EncryptForm>({
    resolver: zodResolver(encryptSchema),
    defaultValues: { keySize: "128", uploadToDrive: false }
  });

  // Decrypt Form
  const { register: registerDecrypt, handleSubmit: handleSubmitDecrypt, formState: { errors: errorsDecrypt } } = useForm<DecryptForm>({
    resolver: zodResolver(decryptSchema)
  });


  const handleAuthorizeDrive = () => {
    window.location.href = 'http://localhost:5000/api/v1/oauth2/authorize-drive';
  };

  const encryptMutation = useMutation({
    mutationFn: async (data: EncryptForm) => {
      const formData = new FormData();
      formData.append('file', data.file[0]);

      // keySize query param
      // uploadToDrive query param
      // shareToList query param

      // Wait, backend expects these as query params?
      // Controller: public async Task<IActionResult> Encrypt(IFormFile file, [FromQuery] int keySize, [FromQuery] bool uploadToDrive, [FromQuery] List<string>? shareToList)
      // Yes, query params.

      const params = new URLSearchParams();
      params.append('keySize', data.keySize);
      params.append('uploadToDrive', (data.uploadToDrive || false).toString());
      if (data.shareToList) {
          // shareToList is List<string>. AspNetCore binds `?shareToList=a&shareToList=b`.
          // If comma separated string is passed, we might need to split.
          // Or backend binds comma separated automatically? No, standard List binding needs multiple params with same name.
          const emails = data.shareToList.split(',').map(s => s.trim()).filter(s => s);
          emails.forEach(email => params.append('shareToList', email));
      }

      const response = await api.post(`/encrypt?${params.toString()}`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
        responseType: 'blob'
      });
      return response;
    },
    onSuccess: (response) => {
        // Download file
        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;

        // Extract filename from Content-Disposition header
        const contentDisposition = response.headers['content-disposition'];
        let fileName = 'encrypted.zip';
        if (contentDisposition) {
            const fileNameMatch = contentDisposition.match(/filename="?([^"]+)"?/);
            if (fileNameMatch && fileNameMatch.length === 2)
                fileName = fileNameMatch[1];
        }

        link.setAttribute('download', fileName);
        document.body.appendChild(link);
        link.click();
        link.remove();
    },
    onError: (error: any) => {
        alert("Encryption failed: " + (error.response?.data?.message || error.message));
    }
  });

  const decryptMutation = useMutation({
    mutationFn: async (data: DecryptForm) => {
        const formData = new FormData();
        formData.append('file', data.file[0]);

        const response = await api.post('/decrypt', formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
            responseType: 'blob'
        });
        return response;
    },
    onSuccess: (response) => {
        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;

        const contentDisposition = response.headers['content-disposition'];
        let fileName = 'decrypted.file';
        if (contentDisposition) {
            const fileNameMatch = contentDisposition.match(/filename="?([^"]+)"?/);
            if (fileNameMatch && fileNameMatch.length === 2)
                fileName = fileNameMatch[1];
        }

        link.setAttribute('download', fileName);
        document.body.appendChild(link);
        link.click();
        link.remove();
    },
    onError: (error: any) => {
        alert("Decryption failed: " + (error.response?.data?.message || error.message));
    }
  });

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="p-4 text-white bg-blue-600 shadow">
        <div className="container flex items-center justify-between mx-auto">
          <h1 className="text-xl font-bold">AES Encryption Dashboard</h1>
          <div className="flex items-center space-x-4">
            <span>Welcome, {user?.username}</span>
            <button onClick={logout} className="px-3 py-1 text-blue-600 bg-white rounded hover:bg-gray-100">Logout</button>
          </div>
        </div>
      </nav>

      <div className="container p-8 mx-auto space-y-8">

        {/* Google Drive Auth Section */}
        <div className="p-6 bg-white rounded shadow">
          <h2 className="mb-4 text-lg font-semibold">Google Drive Integration</h2>
          <p className="mb-4 text-gray-600">Authorize Google Drive to upload encrypted files directly.</p>
          <button
            onClick={handleAuthorizeDrive}
            className="px-4 py-2 text-white bg-red-500 rounded hover:bg-red-600 flex items-center gap-2"
          >
            Authorize / Re-Authorize Drive
          </button>
        </div>

        <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
            {/* Encryption Panel */}
            <div className="p-6 bg-white rounded shadow">
                <h2 className="mb-4 text-lg font-semibold">Encrypt File</h2>
                <form onSubmit={handleSubmitEncrypt((data) => encryptMutation.mutate(data))} className="space-y-4">
                    <div>
                        <label className="block mb-1 text-sm font-medium">Select File</label>
                        <input type="file" {...registerEncrypt('file')} className="w-full text-sm border rounded file:mr-4 file:py-2 file:px-4 file:rounded-l file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100" />
                        {errorsEncrypt.file && <span className="text-xs text-red-500">{errorsEncrypt.file.message as string}</span>}
                    </div>

                    <div>
                        <label className="block mb-1 text-sm font-medium">Key Size</label>
                        <select {...registerEncrypt('keySize')} className="w-full px-3 py-2 border rounded">
                            <option value="128">128 bits</option>
                            <option value="192">192 bits</option>
                            <option value="256">256 bits</option>
                        </select>
                    </div>

                    <div className="flex items-center">
                        <input type="checkbox" {...registerEncrypt('uploadToDrive')} className="mr-2" />
                        <label className="text-sm">Upload to Google Drive</label>
                    </div>

                    {watch('uploadToDrive') && (
                        <div>
                            <label className="block mb-1 text-sm font-medium">Share With (Emails, comma separated)</label>
                            <input {...registerEncrypt('shareToList')} className="w-full px-3 py-2 border rounded" placeholder="email1@example.com, email2@example.com" />
                        </div>
                    )}

                    <button
                        type="submit"
                        disabled={encryptMutation.isPending}
                        className="w-full px-4 py-2 text-white bg-blue-600 rounded hover:bg-blue-700 disabled:bg-blue-300"
                    >
                        {encryptMutation.isPending ? 'Encrypting...' : 'Encrypt & Download'}
                    </button>
                    {encryptMutation.isError && <p className="text-sm text-red-500">Error occurred during encryption.</p>}
                </form>
            </div>

            {/* Decryption Panel */}
            <div className="p-6 bg-white rounded shadow">
                <h2 className="mb-4 text-lg font-semibold">Decrypt File</h2>
                <p className="mb-4 text-sm text-gray-500">Upload the zipped encrypted file (containing meta info) to decrypt.</p>
                <form onSubmit={handleSubmitDecrypt((data) => decryptMutation.mutate(data))} className="space-y-4">
                    <div>
                        <label className="block mb-1 text-sm font-medium">Select Encrypted Zip</label>
                        <input type="file" {...registerDecrypt('file')} accept=".zip" className="w-full text-sm border rounded file:mr-4 file:py-2 file:px-4 file:rounded-l file:border-0 file:text-sm file:font-semibold file:bg-green-50 file:text-green-700 hover:file:bg-green-100" />
                        {errorsDecrypt.file && <span className="text-xs text-red-500">{errorsDecrypt.file.message as string}</span>}
                    </div>

                    <button
                        type="submit"
                        disabled={decryptMutation.isPending}
                        className="w-full px-4 py-2 text-white bg-green-600 rounded hover:bg-green-700 disabled:bg-green-300"
                    >
                        {decryptMutation.isPending ? 'Decrypting...' : 'Decrypt & Download'}
                    </button>
                    {decryptMutation.isError && <p className="text-sm text-red-500">Error occurred during decryption.</p>}
                </form>
            </div>
        </div>
      </div>
    </div>
  );
}
