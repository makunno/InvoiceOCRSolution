import streamlit as st
import requests

st.title("Invoice OCR Parser")

uploaded_file = st.file_uploader("Choose an invoice image", type=["png", "jpg", "jpeg"])

if uploaded_file is not None:
    st.image(uploaded_file, caption="Uploaded Invoice", use_column_width=True)

    if st.button("Parse Invoice"):
        with st.spinner("Calling OCR API..."):

            files = {
                "file": (uploaded_file.name, uploaded_file.getvalue(), uploaded_file.type)
            }

            try:
                response = requests.post("http://localhost:5020/api/invoice/upload", files=files)
                response.raise_for_status()
                data = response.json()

                st.success("Invoice parsed successfully!")
                st.json(data)

                st.write(f"Patient Name: {data.get('patientName')}")
                st.write(f"Bill Number: {data.get('billNumber')}")
                st.write(f"Contact Number: {data.get('contactNumber')}")
                st.write(f"Total Amount: {data.get('totalAmount')}")

            except requests.RequestException as e:
                st.error(f"API call failed: {e}")
